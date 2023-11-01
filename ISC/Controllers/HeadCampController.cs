using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Core.APIDtos;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ISC.Services.ISerivces.IModelServices;
using ISC.Core.ModelsDtos;

namespace ISC.API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles =$"{Roles.LEADER},{Roles.HOC}")]
	public class HeadCampController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IMailServices _MailService;
		private readonly IOnlineJudgeServices _onlineJudgeSrvices;
		private readonly ISheetServices _sheetServices;
		private readonly ISessionsServices _sessionsSrvices;
		public HeadCampController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IUnitOfWork unitofwork, IOnlineJudgeServices onlinejudgeservices, IMailServices mailService, ISheetServices sheetServices)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_UnitOfWork = unitofwork;
			_onlineJudgeSrvices = onlinejudgeservices;
			_MailService = mailService;
			_sheetServices = sheetServices;
		}
		[HttpGet("DisplayTrainees")]
		public async Task<IActionResult> displayTrainees()
		{
			return Ok(_UserManager.GetUsersInRoleAsync(Role.TRAINEE).Result.Select(tr => new
			{
				tr.Id,
				FullName = tr.FirstName + ' ' + tr.MiddleName + ' ' + tr.LastName,
				tr.Email,
				tr.CodeForceHandle,
				tr.Gender,
				tr.Grade
			}));
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
			await _UnitOfWork.completeAsync();
			return Ok();
		}
		[HttpGet("WeeklyFilteration")]
		public async Task<IActionResult> weeklyFilter([FromQuery]List<int> traineesid)
		{
			string? headOfCampUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			Camp? camp = _UnitOfWork.HeadofCamp.findWithChildAsync(t => t.UserId == headOfCampUserId,
																new[] { "Camp", })?.Result?.Camp??null;

			var result =await _sheetServices.TraineeSheetAccesWithout(traineesid, camp?.Id ?? 0);
			if (result.Success == false)
			{
				return BadRequest(result.Comment);
			}
			List<TraineeSheetAccess> traineesAccess = result.Entity;
			var ProblemSheetCount = _sheetServices.TraineeSheetProblemsCount(traineesAccess).Result.Entity;
			var FilteredOnSheets = _sheetServices.TraineesFilter(traineesAccess,ProblemSheetCount).Result.Entity;
			var traineesIds = traineesAccess.Select(i => i.TraineeId).ToList();
			var FilteredOnSessions = _sessionsSrvices.SessionFilter(traineesIds).Result.Entity;
			List<KeyValuePair<FilteredUserDto, string>> Filtered = new List<KeyValuePair<FilteredUserDto, string>>();
			int tsaSize=traineesAccess.Count();
			for(int Trainee = 0; Trainee < tsaSize; Trainee++)
			{
				bool FoundInSheetFilter = FilteredOnSheets?.Contains(traineesAccess[Trainee].TraineeId) ?? false;
				bool FoundInSessionFilter = FilteredOnSessions?.Contains(traineesAccess[Trainee].TraineeId) ?? false;
				if( FoundInSheetFilter || FoundInSessionFilter)
				{
					UserAccount TraineeAccount = await _UserManager.FindByIdAsync(traineesAccess[Trainee].Trainee.UserId);
					if (TraineeAccount != null)
					{
						var FilteredUser = new FilteredUserDto()
						{
							UserId=TraineeAccount.Id,
							FirstName = TraineeAccount.FirstName,
							MiddleName = TraineeAccount.MiddleName,
							LastName = TraineeAccount.LastName,
							Email = TraineeAccount.Email,
							PhoneNumber = TraineeAccount.PhoneNumber,
							CodeforceHandle = TraineeAccount.CodeForceHandle,
							College = TraineeAccount.College,
							Gender = TraineeAccount.Gender,
							Grade = TraineeAccount.Grade
						};
						StringBuilder Reason = new StringBuilder();
						if (FoundInSheetFilter == true)
						{
							Reason.Append("Sheets");
						}
						if (FoundInSessionFilter == true)
						{
							Reason.Append(Reason.Length != 0 ? "/Sessions" : "Sessions");
						}
						Filtered.Add(new(FilteredUser, Reason.ToString()));
					}
				}
			}
			return Ok(Filtered);
		}
		[HttpDelete("SubmitWeeklyFilteration")]
		public async Task<IActionResult> submitWeeklyFilter(List<string> usersid)
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			//Camp Camp = await _UnitOfWork.Camps.getCampByUserIdAsync(userId);
			Camp? Camp = _UnitOfWork.HeadofCamp.findWithChildAsync(t => t.UserId == userId,
																new[] { "Camp", }).Result?.Camp ?? null; 
			List<UserAccount>Fail = new List<UserAccount>();
			foreach (var Id in usersid)
			{
				UserAccount traineeAccount = await _UserManager.FindByIdAsync(Id);
				if (traineeAccount != null)
				{
					TraineeArchive ToArchive = new TraineeArchive()
					{
						FirstName = traineeAccount.FirstName,
						MiddleName = traineeAccount.MiddleName,
						LastName = traineeAccount.LastName,
						NationalID = traineeAccount.NationalId,
						BirthDate = traineeAccount.BirthDate,
						Grade = traineeAccount.Grade,
						College = traineeAccount.College,
						Gender = traineeAccount.Gender,
						CodeForceHandle = traineeAccount.CodeForceHandle,
						FacebookLink = traineeAccount.FacebookLink,
						VjudgeHandle = traineeAccount.VjudgeHandle,
						Email = traineeAccount.Email,
						PhoneNumber = traineeAccount.PhoneNumber,
						Year = Camp.Year,
						CampName = Camp.Name,
						IsCompleted = false
					};
					var Result = await _MailService.sendEmailAsync(traineeAccount.Email, "ICPC Sohag Filteration announcement"
						, $"Hello {traineeAccount.FirstName + ' ' + traineeAccount.MiddleName},{@"<\br>"} We regret to inform you that we had to remove you from the {Camp.Name} training program." +
						$" If you're interested in exploring other training programs, please let us know, and we'll provide you with more information." +
						$" Thank you for your efforts, and we hope you'll take this as a learning experience to continue your growth and development." +
						$"{@"<\br>"}{@"<\br>"}Best regards,{@"<\br>"}{@"<\br>"} ISc System{@"<\br>"}{@"<\br>"} Omar Alaa");
					if (Result == true)
					{
						_UnitOfWork.TraineesArchive.addAsync(ToArchive);
						await _UserManager.DeleteAsync(traineeAccount);
					}
					else
					{
						Fail.Add(traineeAccount);
					}
				}
			}
			await _UnitOfWork.completeAsync();
			return Ok(new { Fail, Comment = Fail.IsNullOrEmpty() ? "" : "please back to system Admin to solve this problem" });
		}
	}
}
