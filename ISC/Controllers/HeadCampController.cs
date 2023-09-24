using ISC.API.APIDtos;
using ISC.API.Helpers;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles =$"{Roles.LEADER},{Roles.HOC}")]
	public class HeadCampController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IMailServices _MailService;
		private readonly IOnlineJudgeServices _OnlineJudgeSrvices;
		private readonly CodeForceConnection _CFConnect;
		public HeadCampController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork,IOnlineJudgeServices onlinejudgeservices, IMailServices mailService,IOptions<CodeForceConnection>cfconnect)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
			_OnlineJudgeSrvices = onlinejudgeservices;
			_MailService = mailService;
			_CFConnect = cfconnect.Value;
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
		[HttpGet("WeeklyFilteration")]
		public async Task<IActionResult> weeklyFilter([FromQuery]List<int> traineesid)
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			Camp Camp=await _UnitOfWork.Camps.getCampByUserIdAsync(userId);
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

			var TraineeSheetAcess = _UnitOfWork.TraineesSheetsAccess
				.findManyWithChildAsync(ts => ts.Sheet.CampId == Camp.Id && (IsFound != null ? !IsFound[ts.TraineeId] : true)
				, new[] { "Sheet", "Trainee" }).Result.OrderBy(ts=>ts.TraineeId).ToList();
			
			Dictionary<int, int> ProblemSheetCount = TraineeSheetAcess
				.DistinctBy(tsa => tsa.SheetId)
				.Select( i => new { i.SheetId,
					Count = _OnlineJudgeSrvices.getContestStandingAsync(i.Sheet.SheetCfId, 1, true,
							i.Sheet.IsSohag?_CFConnect.SohagKey:_CFConnect.AssuitKey
							,i.Sheet.IsSohag?_CFConnect.SohagSecret:_CFConnect.AssuitSecret)
				.Result.result.problems.Count() })
				.ToDictionary(i => i.SheetId, i => i.Count);
			HashSet<int>FilteredOnSheets = new HashSet<int>();
			int TSA_Size = TraineeSheetAcess.Count();
			for (int Trainee = 0; Trainee < TSA_Size; ++Trainee)
			{
				double precent = TraineeSheetAcess[Trainee].NumberOfProblems / (double)ProblemSheetCount[TraineeSheetAcess[Trainee].SheetId];
				int TraineePrecent = ((int)Math.Ceiling(precent * 100.0));
				if (TraineePrecent < TraineeSheetAcess[Trainee].Sheet.MinimumPrecent)
				{
					FilteredOnSheets.Add(TraineeSheetAcess[Trainee].TraineeId);
					int CurrentTrainee = TraineeSheetAcess[Trainee].TraineeId;
					while (Trainee < TSA_Size && TraineeSheetAcess[Trainee].TraineeId == CurrentTrainee) ++Trainee;
					--Trainee;
				}
			}
			var TraineesIds = TraineeSheetAcess.Select(i => i.TraineeId).ToList();
			var TraineeAttendence = _UnitOfWork.TraineesAttendence
					.getInListAsync(tr => TraineesIds.Contains(tr.TraineeId))
								.Result.GroupBy(attend => attend.TraineeId)
								.Select(g => new { TraineeId = g.Key, NumberOfAttendence = g.Count() }).ToList();
			HashSet<int> FilteredOnSessions = new HashSet<int>();
			if (TraineeAttendence.Count > 0)
			{
				int MaxAttendence = TraineeAttendence.Max(ta => ta.NumberOfAttendence);
				FilteredOnSessions = TraineeAttendence
								.Where(i => MaxAttendence - i.NumberOfAttendence > 2)
								.Select(ta => ta.TraineeId).ToHashSet();
			}
			List<KeyValuePair<FilteredUserDto, string>> Filtered = new List<KeyValuePair<FilteredUserDto, string>>();
			for(int Trainee = 0; Trainee < TSA_Size; Trainee++)
			{
				bool FoundInSheetFilter = FilteredOnSheets.Contains(TraineeSheetAcess[Trainee].TraineeId);
				bool FoundInSessionFilter = FilteredOnSessions.Contains(TraineeSheetAcess[Trainee].TraineeId);
				if( FoundInSheetFilter || FoundInSessionFilter)
				{
					UserAccount TraineeAccount = await _UserManager?.FindByIdAsync(TraineeSheetAcess[Trainee]?.Trainee.UserId)??null;
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
			Camp Camp = await _UnitOfWork.Camps.getCampByUserIdAsync(userId);
			List<UserAccount>Fail = new List<UserAccount>();
			foreach (var Id in usersid)
			{
				UserAccount TraineeAccount = await _UserManager.FindByIdAsync(Id);
				if (TraineeAccount != null)
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
					var Result = true; await _MailService.sendEmailAsync(TraineeAccount.Email, "ICPC Sohag Filteration announcement"
						, $"Dear {TraineeAccount.FirstName + ' ' + TraineeAccount.MiddleName},{@"<\br>"} We regret to inform you that we had to remove you from the {Camp.Name} training program." +
						$" If you're interested in exploring other training programs, please let us know, and we'll provide you with more information." +
						$" Thank you for your efforts, and we hope you'll take this as a learning experience to continue your growth and development." +
						$"{@"<\br>"}{@"<\br>"}Best regards,{@"<\br>"}{@"<\br>"} ISc System{@"<\br>"}{@"<\br>"} Omar Alaa");
					if (Result == true)
					{
						_UnitOfWork.TraineesArchive.addAsync(ToArchive);
						await _UserManager.DeleteAsync(TraineeAccount);
					}
					else
					{
						Fail.Add(TraineeAccount);
					}
				}
			}
			await _UnitOfWork.comleteAsync();
			return Ok(new { Fail, Comment = Fail.IsNullOrEmpty() ? "" : "please back to system Admin to solve this problem" });
		}
	}
}
