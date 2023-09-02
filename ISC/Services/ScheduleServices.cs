using CodeforceApiServices;
using ISC.API.APIDtos;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.EF.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using static ISC.API.APIDtos.CodeForcesDtos;

namespace ISC.API.Services
{
	public class ScheduleSerives
	{
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IOnlineJudgeServices _OnlineJudge;
		private readonly UserManager<UserAccount> _UserManager;
		public ScheduleSerives(IUnitOfWork unitofwork, IOnlineJudgeServices onlinejudge, UserManager<UserAccount> usermanger)
		{
			_UnitOfWork = unitofwork;
			_OnlineJudge = onlinejudge;
			_UserManager = usermanger;
		}
		public async Task<int> updateTraineeSolveCurrentAccessAsync()
		{
			  Console.WriteLine("enter");
			int EffectedRows = 0;
			var TraineeSheets = await _UnitOfWork.TraineesSheetsAccess.getAllAsync();
			var SheetsIds = TraineeSheets.DistinctBy(STA => STA.SheetId).Select(STA=>STA.SheetId);
			var TraineesIds = TraineeSheets.DistinctBy(STA => STA.TraineeId).Select(STA=>STA.TraineeId);
			Dictionary<int, CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> SheetsSubmissions =
				new Dictionary<int, CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>();
			Dictionary<int, string> AccountsHandles = new Dictionary<int, string>();
			foreach(var sid in SheetsIds)
			{
				var SheetInfo = await _UnitOfWork.Sheets.getByIdAsync(sid);
				if (!SheetsSubmissions.ContainsKey(sid))
				{
					SheetsSubmissions[sid] =await _OnlineJudge.getContestStatusAsync(SheetInfo.SheetCfId, "");
				}
			}
			foreach(var tid in TraineesIds)
			{
				var TraineeInfo=await _UnitOfWork.Trainees.getByIdAsync(tid);
				if (!AccountsHandles.ContainsKey(tid))
				{
					var CodeForceHandle = _UserManager.Users.Where(u => u.Id == TraineeInfo.UserId)
									.Select(user => user.CodeForceHandle)?
									.FirstOrDefault()?.ToString();
					if(CodeForceHandle != null)
					AccountsHandles[tid] = CodeForceHandle;
				}
			}

			foreach (var Access in TraineeSheets)
			{
				int Old = Access.NumberOfProblems;
				int Changes = SheetsSubmissions[Access.SheetId].
					result.Where(
					s => s.verdict == "OK" &&
					s.author.members.Exists(m => m.name == AccountsHandles[Access.TraineeId])
					)
					.Count();
				EffectedRows += Changes ==  Old? 0 : 1;
				Access.NumberOfProblems = Changes;
			}
			await _UnitOfWork.comleteAsync();
			Console.WriteLine(EffectedRows);
			return EffectedRows;
		}
	}
}
