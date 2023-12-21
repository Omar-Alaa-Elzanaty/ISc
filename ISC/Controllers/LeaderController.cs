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
using ISC.Services.Services.ExceptionSerivces.Exceptions;

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
		private readonly IMediaServices _mediaService;
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
			ICampServices campServices,
			IMediaServices mediaService)
		{
			_roleManager = roleManager;
			_userManager = userManager;
			_unitOfWork = unitofwork;
			_auth = auth;
			_leaderServices = leaderServices;
			_sheetServices = sheetServices;
			_mapper = mapper;
			_campServices = campServices;
			_mediaService = mediaService;
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
				FullName=acc.FirstName+' '+acc.MiddleName+' '+acc.LastName,
				acc.CodeForceHandle,
				acc.Email,
				acc.College,
				CampName = _unitOfWork.Trainees.getCampofTrainee(acc.Id)?.Result?.Name
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
			var resp = await _leaderServices.AssignRoleToStuff(model);

			return Ok(resp);
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteFromStuff(List<string> stuffusersId)
		{
			return Ok(await _leaderServices.DeleteStuffAsync(stuffusersId));
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteFromTrainees([FromBody] List<DeleteTraineeDto> trainees)
		{
			 await _leaderServices.DeleteTraineesAsync(trainees);
			return Ok();
		}

		[HttpPost]
		public async Task<IActionResult> AddCamp([FromBody] CampDto camp)
		{
			var response = await _leaderServices.AddCampAsync(camp);
			if (!response.IsSuccess)
			{
				return BadRequest(new
				{
					response.IsSuccess,
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

			return Ok(response.IsSuccess);
		}
		[HttpPost]
		public async Task<IActionResult> AddRole([FromBody] string role)
		{
			var response = new ServiceResponse<bool>();
			
			var result = await _roleManager.FindByNameAsync(role);

			if (result != null)
			{
				throw new BadRequestException($"Role {role} is already exist!");
			}

			var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
			if (!roleResult.Succeeded)
			{
				var errors=" ";
				foreach(var error in roleResult.Errors)
				{
					errors += $"{error.Description}\n";
				}

				throw new BadRequestException(errors);
			}

			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> DisplayTraineeArchive()
		{
			var response = new ServiceResponse<List<TraineeArchive>>() { IsSuccess = true };

			response.Entity = await _unitOfWork.TraineesArchive.getAllAsync();

			return Ok(response);
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteTraineeArchive([FromBody] List<string> members)
		{
			return Ok(await _leaderServices.DeleteTraineeArchivesAsync(members));
		}
		[HttpPut]
		public async Task<IActionResult> UpdateTraineeArchive([FromBody] HashSet<TraineeArchiveDto> archives)
		{
			await _leaderServices.UpdateTraineeArchive(archives);

			return Ok("Success");
		}
		[HttpGet]
		public async Task<IActionResult> DisplayStuffArchive()
		{
			return Ok(await _unitOfWork.StuffArchive.getAllAsync());
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
		public async Task<IActionResult> UpdateStuffArchive(HashSet<StuffArchiveDto> archives)
		{
			await _leaderServices.UpdateStuffArchive(archives);

			return Ok("Update Successfully");
		}
		[HttpGet("{campId}")]
		public async Task<IActionResult> DisplayNewRegister(int campId)
		{
			return Ok(await _leaderServices.DisplayNewRegisterAsync(campId));
		}
		//TODO: start from here
		[HttpPost]
		public async Task<IActionResult> SubmitNewRegisters(SubmitNewRegisterDto newRegisters)
		{
			HashSet<string> refused = new HashSet<string>();

			foreach (var contest in newRegisters.ContestsInfo)
			{
				var standingResponse = await _sheetServices.SheetStanding(contest.ContestId, contest.IsSohag);
				if (!standingResponse.IsSuccess)
				{
					return BadRequest(standingResponse.Comment);
				}
				var statusResponse = await _sheetServices.SheetStatus(contest.ContestId, contest.IsSohag);
				if (!statusResponse.IsSuccess)
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
				newTrainee.Role=Role.TRAINEE;
				newTrainee.CampId = newRegisters.CampId;

				var response = await _leaderServices.AutoMemberAddAsync(
					registerDto: newTrainee,
					campName: camp
					);

				if (!response.IsSuccess)
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
			if (!response.IsSuccess)
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
	}
}
