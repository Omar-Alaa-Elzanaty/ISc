using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.EF.Repositories;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using static ISC.Core.APIDtos.CodeForcesDtos;

namespace ISC.Services.Services
{
	public class ScheduleSerives
	{
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IOnlineJudgeServices _OnlineJudge;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly CodeForceConnection _CFConnection;
		public ScheduleSerives(IUnitOfWork unitofwork, IOnlineJudgeServices onlinejudge, UserManager<UserAccount> usermanger,IOptions<CodeForceConnection>cfconnection)
		{
			_UnitOfWork = unitofwork;
			_OnlineJudge = onlinejudge;
			_UserManager = usermanger;
			_CFConnection = cfconnection.Value;
		}
		public async Task<int> updateTraineeSolveCurrentAccessAsync()
		{
			Console.WriteLine("enter");
			int EffectedRows = 0;
			var TraineeSheets = await _UnitOfWork.TraineesSheetsAccess.getAllAsync();
			var SheetsIds = TraineeSheets.DistinctBy(STA => STA.SheetId).Select(sta => sta.SheetId);
			var TraineesIds = TraineeSheets.DistinctBy(STA => STA.TraineeId).Select(sta => sta.TraineeId);

			Dictionary<int, CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> SheetsSubmissions =
				new Dictionary<int, CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>();

			Dictionary<int, string> AccountsHandles = new Dictionary<int, string>();

			foreach(var sid in SheetsIds)
			{
				var SheetInfo = await _UnitOfWork.Sheets.getByIdAsync(sid);
				if (!SheetsSubmissions.ContainsKey(sid))
				{
					SheetsSubmissions[sid] =await _OnlineJudge.GetContestStatusAsync(SheetInfo.SheetCfId,
						SheetInfo.IsSohag?_CFConnection.SohagKey:_CFConnection.AssuitKey
						,SheetInfo.IsSohag?_CFConnection.SohagSecret:_CFConnection.AssuitSecret,100);
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
				int Changes = SheetsSubmissions[Access.SheetId].
					result.Where(
					s => s.verdict == "OK" &&
					s.author.members.Exists(m => m.name == AccountsHandles[Access.TraineeId])
					)
					.Count();
				EffectedRows += Changes == Access.NumberOfProblems ? 0 : 1;
				Access.NumberOfProblems = Changes;
			}
			await _UnitOfWork.completeAsync();
			Console.WriteLine(EffectedRows);
			return EffectedRows;
		}
	}
}
