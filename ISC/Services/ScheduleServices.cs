using ISC.API.APIDtos;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using static ISC.API.APIDtos.CodeForcesDtos;

namespace ISC.API.Services
{
	internal class ScheduleSerives
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
			var Accessing = await _UnitOfWork.TraineesSheetsAccess.getAllwithNavigationsAsync(new[] { "Trainee", "Sheet" });
			Dictionary<int, CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> SheetsSubmissions =
				new Dictionary<int, CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>();
			Dictionary<int, string> AccountsHandles = new Dictionary<int, string>();
			foreach (var Access in Accessing)
			{
				if (!SheetsSubmissions.ContainsKey(Access.SheetId))
				{
					CodeforcesApiResponseDto<List<CodeforceSubmisionDto>> submission = await _OnlineJudge.getContestStatusAsync(Access.Sheet.SheetCfId, "");
					if (submission != null)
					{
						SheetsSubmissions.TryAdd(Access.SheetId, submission);
					}
				}
				if (!AccountsHandles.ContainsKey(Access.TraineeId))
				{
					var Account = await _UserManager.FindByIdAsync(Access.Trainee.UserId);
					if (Account != null)
					{
						AccountsHandles.TryAdd(Access.TraineeId, Account.Id);
					}
				}
			}
			if (SheetsSubmissions.Count == 0 || AccountsHandles.Count == 0)
			{
				return -404;
			}
			foreach (var Access in Accessing)
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
			await _UnitOfWork.comleteAsync();
			Console.WriteLine(EffectedRows);
			return EffectedRows;
		}
		public void test()
		{
			//updateTraineeSolveCurrentAccessAsync();

		}
	}
}
