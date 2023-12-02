using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Core.APIDtos;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ISC.Services.ISerivces.IModelServices;
using ISC.Core.Dtos;
using AutoMapper;
using Microsoft.Identity.Client;
using ISC.Services.Services.ModelSerivces;

namespace ISC.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	//[Authorize(Roles = $"Admin, {Role.LEADER}")]
	public class LeaderController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<UserAccount> _userManager;
		private readonly IAuthanticationServices _auth;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILeaderServices _leaderServices;
		private readonly ISheetServices _sheetServices;
		private readonly ICampServices _campServices;
		private readonly IMapper _mapper;
		public LeaderController(
			RoleManager<IdentityRole> roleManager,
			UserManager<UserAccount> userManager,
			IAuthanticationServices auth,
			IUnitOfWork unitofwork,
			IMailServices mailServices,
			ILeaderServices leaderServices,
			ISheetServices sheetServices,
			IMapper mapper,
			ICampServices campServices)
		{
			_roleManager = roleManager;
			_userManager = userManager;
			_unitOfWork = unitofwork;
			_auth = auth;
			_leaderServices = leaderServices;
			_sheetServices = sheetServices;
			_mapper = mapper;
			_campServices = campServices;
		}

		[HttpGet]
		public async Task<IActionResult> DisplaySystemRoles()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			return Ok(roles.Select(role => role.Name));
		}

		[HttpGet]
		public async Task<IActionResult> DisplayStuff()
		{
			var Accounts = _userManager.Users.ToList();
			var TraineeAccounts = await _userManager.GetUsersInRoleAsync(Role.TRAINEE);
			var response = Accounts.Except(TraineeAccounts).Select(acc => new
			{
				acc.Id,
				FullName = acc.FirstName + ' ' + acc.MiddleName + ' ' + acc.LastName,
				acc.CodeForceHandle,
				acc.Email
			}).ToList();
			return Ok(response);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayTrainee()
		{
			var response = _userManager.GetUsersInRoleAsync(Role.TRAINEE).Result.Select(acc => new
			{
				acc.Id,
				acc.CodeForceHandle,
				acc.Email,
				acc.College,
				CampName = _unitOfWork.Trainees.getCampofTrainee(acc.Id).Result.Name
			});
			return Ok(response);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayAll()
		{

			var Accounts = await _userManager.Users.Select(i => new
			{
				i.Id,
				i.UserName,
				FullName = i.FirstName + ' ' + i.MiddleName + ' ' + i.LastName,
				Role = new List<string>(),
				i.CodeForceHandle,
				i.Email,
				i.College,
				i.Gender,
				i.PhoneNumber
			}).ToListAsync();

			foreach (var acc in Accounts)
			{
				var userAccount = await _userManager.FindByIdAsync(acc.Id);
				acc.Role.AddRange(_userManager.GetRolesAsync(userAccount).Result.ToList());
			}

			return Ok(Accounts);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayAllExceptHeadOfTraining()
		{
			var HocUserId = _unitOfWork.HeadofCamp.getAllAsync().Result.Select(hoc => hoc.UserId).ToList();
			var StuffWithoutHoc = _userManager.Users.Where(user => HocUserId.Contains(user.Id) == false).ToList();
			return Ok(StuffWithoutHoc);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayCamp()
		{
			return Ok(await _campServices.DisplayCampsDetails());
		}

		[HttpPost]
		public async Task<IActionResult> Register([FromForm] RegisterDto newUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var model = await _auth.RegisterAsync(user: newUser, sendEmail: true);
			if (!model.IsAuthenticated)
			{
				return BadRequest(model.Message);
			}
			return Ok(new
			{
				model.Token,
				model.ExpireOn,
				model.UserId
			});
		}

		[HttpPost]
		public async Task<IActionResult> AssignToStuffRoles([FromBody] StuffNewRolesDto model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("some data is required");
			}
			var Account = await _userManager.FindByIdAsync(model.UserId);
			if (Account == null)
			{
				return BadRequest("there is no Account with these properities!");
			}
			List<string> ErrorList = new List<string>();
			foreach (var Role in model.UserRoles)
			{
				bool Result = await _unitOfWork.addToRoleAsync(Account, Role.Role, Role.CampId, Role.MentorId);
				if (Result == false)
					_ = ErrorList.Append(Role.Role + ',');
			}
			await _unitOfWork.completeAsync();
			if (ErrorList.Count != 0) {
				return BadRequest($"Can't save user to these roles{ErrorList}");
			}
			return Ok("Changes have been successfully");

		}

		[HttpDelete]
		public async Task<IActionResult> DeleteFromStuff(List<string> stuffusersId)
		{
			List<UserAccount> ErrorsList = new List<UserAccount>();
			foreach (string UserId in stuffusersId)
			{
				var Account = await _userManager.FindByIdAsync(UserId);
				var UserRoles = _userManager.GetRolesAsync(Account).Result.ToList();
				bool result = true;
				if (UserRoles.Contains(Role.MENTOR))
				{
					result = await _unitOfWork.Mentors.deleteAsync(UserId);
				}
				if (UserRoles.Contains(Role.HOC) && result == true)
				{
					result = await _unitOfWork.HeadofCamp.deleteEntityAsync(UserId);
				}
				if (result == true)
				{
					StuffArchive Archive = new StuffArchive()
					{
						FirstName = Account.FirstName,
						MiddleName = Account.MiddleName,
						LastName = Account.LastName,
						NationalID = Account.NationalId,
						BirthDate = Account.BirthDate,
						Grade = Account.Grade,
						College = Account.College,
						Gender = Account.Gender,
						CodeForceHandle = Account.CodeForceHandle,
						FacebookLink = Account.FacebookLink,
						VjudgeHandle = Account.VjudgeHandle,
						Email = Account.Email,
						PhoneNumber = Account.PhoneNumber
					};
					await _unitOfWork.StuffArchive.addAsync(Archive);
					await _userManager.DeleteAsync(Account);
				}
				else
					ErrorsList.Append(Account);
			}
			await _unitOfWork.completeAsync();
			return Ok(ErrorsList);
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteFromTrainees([FromBody] List<string> traineesIds)
		{
			var response = await _leaderServices.DeleteTraineesAsync(traineesIds);
			if (!response.Success)
			{
				return Ok(new
				{
					response.Success,
					response.Comment
				});
			}
			return Ok(response);
		}

		[HttpPost]
		public async Task<IActionResult> AddCamp([FromBody] CampDto camp)
		{
			var response = await _leaderServices.AddCampAsync(camp);
			if (!response.Success)
			{
				return BadRequest(new
				{
					response.Success,
					response.Comment
				});
			}
			return Ok("Success");
		}

		[HttpGet]
		public async Task<IActionResult> RoleUserDisplay()
		{
			var accounts = _userManager.Users.Select(i => new
			{
				i.Id,
				FullName = i.FirstName + ' ' + i.MiddleName + " " + i.LastName,
				i.CodeForceHandle,
				i.Email,
				i.College,
				i.Gender
			});
			return Ok(accounts);
		}

		[HttpPost]
		public async Task<IActionResult> AddToRole([FromBody] UserRoleDto users)
		{
			var response = await _leaderServices.AddToRoleAsync(users);
			if (!response.Success)
			{
				return Ok(response);
			}
			return Ok(response.Success);
		}
		[HttpPost]
		public async Task<IActionResult> AddRole([FromBody] string role)
		{
			var result = await _roleManager.FindByNameAsync(role);
			if (result != null)
			{
				return BadRequest($"Role {role} is already exist!");
			}
			result = new IdentityRole() { Name = role };
			var response = await _roleManager.CreateAsync(result);
			if (!response.Succeeded)
			{
				return BadRequest(response.Errors);
			}

			return Ok("Add successful");
		}
		[HttpGet]
		public async Task<IActionResult> DisplayTraineeArchive()
		{
			var response = await _unitOfWork.TraineesArchive.getAllAsync();
			return Ok(response);
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteTraineeArchive([FromBody] List<string> members)
		{
			if (members == null || members.Count() == 0)
			{
				return BadRequest("Invalid request");
			}
			var trainees = await _unitOfWork.TraineesArchive.findManyWithChildAsync(ta => members.Contains(ta.NationalID));
			if (trainees == null || trainees.Count == 0)
			{
				return BadRequest("No account to remove");
			}
			_unitOfWork.TraineesArchive.deleteGroup(trainees);
			_ = await _unitOfWork.completeAsync();
			return Ok("Deleted Successfully");
		}
		[HttpPut]
		public async Task<IActionResult> UpdateTraineeArchive([FromBody] List<TraineeArchiveDto> archives)
		{
			var nationalIds = archives.Select(a => a.NationalId);
			var members = await _unitOfWork.TraineesArchive.findManyWithChildAsync(ta => nationalIds.Contains(ta.NationalID));
			foreach (var archive in archives)
			{
				var trainee = members.Single(m => m.NationalID == archive.NationalId);
				var name = archive.FullName.Split(' ');
				if (name.Length < 3)
				{
					return BadRequest("Full name is not valid");
				}
				trainee.FirstName = name[0];
				trainee.MiddleName = name[1];
				trainee.LastName = name[2];
				trainee.College = archive.College;
				trainee.CodeForceHandle = archive.CodeforceHandle;
				trainee.FacebookLink = archive.FacebookLink;
				trainee.Email = archive.Email;
				trainee.VjudgeHandle = archive.VjudgeHandle;
				trainee.BirthDate = archive.BirthDate;
				trainee.Year = archive.Year;
				trainee.IsCompleted = archive.IsCompleted;
				trainee.PhoneNumber = archive.PhoneNumber;
			}
			_ = await _unitOfWork.completeAsync();
			return Ok("Success");
		}
		[HttpGet]
		public async Task<IActionResult> DisplayStuffArchive()
		{
			return Ok(_unitOfWork.StuffArchive.getAllAsync().Result);
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteStuffArchive([FromBody] List<string> members)
		{
			var archives = await _unitOfWork.StuffArchive.getAllAsync(sa => members.Contains(sa.NationalID));
			if (archives.Count == 0)
			{
				return Ok("No Archive to delete");
			}
			_unitOfWork.StuffArchive.deleteGroup(archives);
			_ = await _unitOfWork.completeAsync();
			return Ok("Deleted successfully");
		}
		[HttpPut]
		public async Task<IActionResult> UpdateStuffArchive(List<StuffArchiveDto> archives)
		{
			var nationalIds = archives.Select(a => a.NationalID).ToList();
			var members = await _unitOfWork.StuffArchive.findManyWithChildAsync(sa => nationalIds.Contains(sa.NationalID));
			foreach (var stuffMember in archives) {
				var stuff = members.Single(m => m.NationalID == stuffMember.NationalID);
				var name = stuffMember.FullName.Split(' ');
				stuff.FirstName = name[0];
				stuff.MiddleName = name[1];
				stuff.LastName = name[2];
				stuff.NationalID = stuffMember.NationalID;
				stuff.BirthDate = stuffMember.BirthDate;
				stuff.Grade = stuffMember.Grade;
				stuff.College = stuffMember.College;
				stuff.Gender = stuffMember.Gender;
				stuff.CodeForceHandle = stuffMember.CodeForceHandle;
				stuff.FacebookLink = stuffMember.FacebookLink;
				stuff.VjudgeHandle = stuffMember.VjudgeHandle;
				stuff.Email = stuffMember.Email;
				stuff.PhoneNumber = stuffMember.PhoneNumber;
				stuff.Year = stuffMember.Year;
			}
			await _unitOfWork.completeAsync();
			return Ok("Update Successfully");
		}
		[HttpGet("{campId}")]
		public async Task<IActionResult> DisplayNewRegister(int campId)
		{
			var response = await _leaderServices.DisplayNewRegisterAsync(campId);
			if (!response.Success)
				return BadRequest("No entity");
			return Ok(response);
		}
		[HttpPost]
		public async Task<IActionResult> SubmitNewRegisters(SubmitNewRegisterDto newRegisters)
		{
			HashSet<string> refused = new HashSet<string>();

			foreach (var contest in newRegisters.ContestsInfo)
			{
				var standingResponse = await _sheetServices.SheetStanding(contest.ContestId, contest.IsSohag);
				if (!standingResponse.Success)
				{
					return BadRequest(standingResponse.Comment);
				}
				var statusResponse = await _sheetServices.SheetStatus(contest.ContestId, contest.IsSohag);
				if (!statusResponse.Success)
				{
					return BadRequest(statusResponse.Comment);
				}


				var memberPerProblem = statusResponse.Entity.GroupBy(submission =>
				new
				{
					Handle = submission.author.members.FirstOrDefault().handle,
					ProblemName = submission.problem.name
				}).Where(mps => mps.Any(sub => sub.verdict == "OK")).Select(mps => new
				{
					mps.Key.ProblemName,
					mps.Key.Handle
				}).GroupBy(mps => mps.Handle).Select(problemSolved => new
				{
					handle = problemSolved.Key,
					Count = problemSolved.Count()
				});

				float totalproblems = standingResponse.Entity.problems.Count();

				foreach (var member in memberPerProblem) {
					if (Math.Ceiling(member.Count / totalproblems) * 100.0 < contest.PassingPrecent)
					{
						refused.Add(member.handle);
					}
				}
			}

			var PassedMember = await _unitOfWork.NewRegitseration
				.findManyWithChildAsync(nr => !refused.Contains(nr.CodeForceHandle)
										&& newRegisters.CandidatesNationalId.Contains(nr.NationalID) == true);

			var camp = _unitOfWork.Camps.getByIdAsync(newRegisters.CampId).Result.Name;

			List<NewRegistration> faillRegisteration = new List<NewRegistration>();
			List<NewRegistration> confirmedAcceptacne = new List<NewRegistration>();

			foreach (var member in PassedMember)
			{
				var newTrainee = _mapper.Map<RegisterDto>(member);
				newTrainee.Roles.Add(Role.TRAINEE);
				newTrainee.CampId = newRegisters.CampId;

				var response = await _leaderServices.AutoMemberAddAsync(
					registerDto: newTrainee,
					campName: camp
					);

				if (!response.Success)
				{
					faillRegisteration.Add(member);
				}
				else
				{
					PassedMember.Add(member);
				}
			}

			_unitOfWork.NewRegitseration.deleteGroup(PassedMember);

			return Ok(faillRegisteration.Select(t => new
			{
				t.NationalID,
				t.FirstName,
				t.MiddleName,
				t.LastName,
				t.Email,
				t.CodeForceHandle
			}));
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteNewRegister([FromBody] List<string> Ids)
		{
			if (Ids == null || Ids.Count == 0)
			{
				return BadRequest("Invalid request");
			}
			var response = await _leaderServices.DeleteFromNewRegister(Ids);
			if (!response.Success)
			{
				return BadRequest();
			}
			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> CampRegisterStatus()
		{
			return Ok(_unitOfWork.Camps.Get().Select(c => new
			{
				c.Name,
				state = c.OpenForRegister
			}));
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateCampState(int id)
		{
			var camp = await _unitOfWork.Camps.getByIdAsync(id);
			if (camp != null)
			{
				camp.OpenForRegister = !camp.OpenForRegister;

				_ = await _unitOfWork.completeAsync();
				return Ok($"State change to {camp.OpenForRegister}");
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}
		[HttpGet]
		public async Task<IActionResult> DisplayOpenedCamp()
		{
			return Ok(_unitOfWork.Camps.findManyWithChildAsync(c => c.OpenForRegister).Result.Select(c => new
			{
				c.Id,
				c.Name
			}));
		}
		[HttpPut]
		public async Task<IActionResult> UpdateCampHead(int headId, int campId, bool toCamp)
		{
			HeadOfTraining hoc = await _unitOfWork.HeadofCamp.getByIdAsync(headId);

			if (toCamp)
			{
				hoc.CampId = campId;
			}
			else
			{
				await _unitOfWork.HeadofCamp.deleteAsync(hoc);
			}
			await _unitOfWork.completeAsync();

			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> AttendenceAccessPage()
		{
			var availableMentors = await _userManager.Users.Include(u => u.Mentor)
									.Where(u => u.Mentor != null).Select(u => new
									{
										u.Mentor.Id,
										FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName,
									}).ToListAsync();

			var sessions = await _unitOfWork.Sessions.Get()
							.Select(s => new
							{
								s.Id,
								s.Topic
							}).ToListAsync();

			return Ok(new { availableMentors, sessions });
		}
		[HttpPut]
		public async Task<IActionResult> GiveAttendenceAccess(int mentorId,int sessionId)
		{
			var mentor = await _unitOfWork.Mentors.getByIdAsync(mentorId);
			var session= await _unitOfWork.Sessions.getByIdAsync(sessionId);

			if (mentor is null || session is null)
			{
				return BadRequest("Invalid request");
			}

			mentor.AccessSessionId = sessionId;

			_= await _unitOfWork.completeAsync();

			return Ok("Success");
		}
		[HttpGet]
		public async Task<IActionResult> GeneralStanding()
		{
			return Ok(await _leaderServices.GeneralStandingsAsync());
		}
	}
}
