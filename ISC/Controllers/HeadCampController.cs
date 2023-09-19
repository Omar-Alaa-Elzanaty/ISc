using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles =$"{Roles.LEADER},{Roles.HOC}")]
	public class HeadCampController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IOnlineJudgeServices _OnlineJudgeSrvices;
		public HeadCampController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork,IOnlineJudgeServices onlinejudgeservices)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
			_OnlineJudgeSrvices = onlinejudgeservices;
		}
		[HttpGet("DisplayTrainees")]
		public async Task<IActionResult> displayTrainee()
		{
			return Ok(await _UserManager.GetUsersInRoleAsync(Roles.TRAINEE));
		}
		[HttpDelete("DeleteFromTrainees")]
		public async Task<IActionResult> deleteFromTrainees(List<string> traineesusersid)
		{
			foreach (string traineeuserid in traineesusersid)
			{
				var TraineeAccount = await _UserManager.Users.Include(i => i.Trainee).Where(user => user.Id == traineeuserid).SingleOrDefaultAsync();
				if (TraineeAccount != null)
				{
					var Camp = await _UnitOfWork.Trainees.getCampofTrainee(TraineeAccount.Trainee.Id);
					TraineeArchive Archive = new TraineeArchive()
					{
						FirstName = TraineeAccount.FirstName,
						MiddleName = TraineeAccount.MiddleName,
						LastName = TraineeAccount.LastName,
						NationalID = TraineeAccount.NationalId,
						BirthDate = TraineeAccount.BirthDate,
						Grade = TraineeAccount.Grade,
						Gender = TraineeAccount.Gender,
						College = TraineeAccount.College,
						CodeForceHandle = TraineeAccount.CodeForceHandle,
						FacebookLink = TraineeAccount.FacebookLink,
						VjudgeHandle = TraineeAccount.VjudgeHandle,
						Email = TraineeAccount.Email,
						PhoneNumber = TraineeAccount.PhoneNumber,
						CampName = Camp.Name,
						IsCompleted = false
					};
					_UnitOfWork.TraineesArchive.addAsync(Archive);
					await _UserManager.DeleteAsync(TraineeAccount);
				}
			}
			await _UnitOfWork.comleteAsync();
			return Ok();
		}
		[HttpDelete("WeeklyFilteration")]
		public async Task<IActionResult> weeklyFilter(List<int> traineesid,int campid)
		{
			var TraineeSheetAcess =await _UnitOfWork.TraineesSheetsAccess.getAllwithNavigationsAsync(new[] { "Sheet","Trainee" });
			bool[]? IsFound;
			if (traineesid.Count() > 0)
			{
				IsFound = new bool[traineesid.Max() + 1];
				int size = traineesid.Count();
				for (int i = 0; i < size; i++)
				{
					IsFound[traineesid[i]] = true;
				}
			}
			else IsFound = null;
			TraineeSheetAcess = TraineeSheetAcess
								.Where(ts => ts.Sheet.CampId == campid &&(IsFound!=null?!IsFound[ts.TraineeId]:true))
								.OrderBy(ts=>ts.TraineeId)
								.ToList();
			Camp Camp =await _UnitOfWork.Camps.getByIdAsync(campid);
			Dictionary<int,int> ProblemSheetCount = TraineeSheetAcess
				.DistinctBy(tsa=>tsa.SheetId)
				.Select(async i => new {i.SheetId,
				Count=_OnlineJudgeSrvices.getContestStandingAsync(i.Sheet.SheetCfId,1,false)
				.Result.result.problems.Count()})
				.ToDictionary(i=>i.Result.SheetId,i=>i.Result.Count);
			List<TraineeSheetAccess> RedFlagTrainees = new List<TraineeSheetAccess>();
			int TSASize = TraineeSheetAcess.Count();
			for(int Trainee=0;Trainee<TSASize;Trainee++)
			{
				double precent = ((double)TraineeSheetAcess[Trainee].NumberOfProblems / (double)ProblemSheetCount[TraineeSheetAcess[Trainee].SheetId]);
				var TraineePrecent = Math.Ceiling(precent * 100);
				if (TraineePrecent < TraineeSheetAcess[Trainee].Sheet.MinimumPrecent)
				{
					RedFlagTrainees.Add(TraineeSheetAcess[Trainee]);
					int CurrentTrainee = TraineeSheetAcess[Trainee].TraineeId;
					while (Trainee < TSASize && TraineeSheetAcess[Trainee].TraineeId == CurrentTrainee) ++Trainee;
					--Trainee;
				}
			}
			List<TraineeArchive> Filtered=new List<TraineeArchive>();
			foreach(var Trainee in RedFlagTrainees)
			{
				UserAccount TraineeAccount =await _UserManager.FindByIdAsync(Trainee.Trainee.UserId);
				if(TraineeAccount != null)
				{
					TraineeArchive ToArchive = new TraineeArchive()
					{
						FirstName = TraineeAccount.FirstName,
						MiddleName = TraineeAccount.MiddleName,
						LastName = TraineeAccount.LastName,
						NationalID = TraineeAccount.NationalId,
						BirthDate = TraineeAccount.BirthDate,
						Grade = TraineeAccount.Grade,
						College = TraineeAccount.College,
						Gender = TraineeAccount.Gender,
						CodeForceHandle = TraineeAccount.CodeForceHandle,
						FacebookLink = TraineeAccount.FacebookLink,
						VjudgeHandle = TraineeAccount.VjudgeHandle,
						Email = TraineeAccount.Email,
						PhoneNumber = TraineeAccount.PhoneNumber,
						Year = Camp.Year,
						CampName = Camp.Name,
						IsCompleted = false
					};
					_UnitOfWork.TraineesArchive.addAsync(ToArchive);
					Filtered.Add(ToArchive);
					await _UserManager.DeleteAsync(TraineeAccount);
				}
			}
			await _UnitOfWork.comleteAsync();
			return Ok(Filtered);
		}
	}
}
