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
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;

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
			ServiceResponse<List<string?>> response = new ServiceResponse<List<string?>>();

			var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
			if (roles.IsNullOrEmpty())
			{
				throw new BadRequestException("No role found");
			}
			else
			{
				response.Entity = roles;
			}

			return Ok(response);
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

			await Task.CompletedTask;
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
			var StuffWithoutHoc = await _userManager.Users.Where(user => HocUserId.Contains(user.Id) == false).ToListAsync();

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

			return Ok(new ServiceResponse<object>()
			{
				IsSuccess = true,
				Entity = new
				{
					model.Token,
					model.ExpireOn,
					model.UserId
				}
			});
		}


		[HttpPost]
		public async Task<IActionResult> AssignToStuffRoles([FromBody] StuffNewRolesDto model)
		{ 
			return Ok(await _leaderServices.AssignRoleToStuff(model));
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteFromStuff(List<string> stuffusersId)
		{
			return Ok(await _leaderServices.DeleteStuffAsync(stuffusersId));
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteFromTrainees([FromBody] List<DeleteTraineeDto> trainees)
		{
			return Ok(await _leaderServices.DeleteTraineesAsync(trainees));
		}

		[HttpPost]
		public async Task<IActionResult> AddCamp([FromBody] CampDto camp)
		{
			return Ok(await _leaderServices.AddCampAsync(camp));
		}

		[HttpGet]
		public async Task<IActionResult> SystemUserDisplay()

		{
			ServiceResponse<object> response = new ServiceResponse<object>();
			response.Entity = _userManager.Users.Select(i => new
			{
				i.Id,
				FullName = i.FirstName + ' ' + i.MiddleName + " " + i.LastName,
				i.CodeForceHandle,
				i.Email,
				i.College,
				i.Gender
			});

			return Ok(response);
		}
		[HttpPost]
		public async Task<IActionResult> AddToRole([FromBody] UserRoleDto users)
		{

			return Ok(await _leaderServices.AddToRoleAsync(users));
		}
		[HttpPost]
		public async Task<IActionResult> AddRole([FromBody] string role)
		{
			var response = new ServiceResponse<bool>() { IsSuccess = true };
			
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

			return Ok(response);
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
			return Ok(await _leaderServices.UpdateTraineeArchive(archives));
		}
		[HttpGet]
		public async Task<IActionResult> DisplayStuffArchive()
		{
			return Ok(await _unitOfWork.StuffArchive.getAllAsync());
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteStuffArchive([FromBody] List<string> members)
		{
			var response = new ServiceResponse<string>();
			var archives = await _unitOfWork.StuffArchive.getAllAsync(sa => members.Contains(sa.NationalID));
			if (archives.Count == 0)
			{
				throw new BadRequestException("No Archive to delete");
			}
			_unitOfWork.StuffArchive.deleteGroup(archives);
			_ = await _unitOfWork.completeAsync();

			response.IsSuccess = true;
			response.Comment= "Deleted successfully";

			return Ok(response);
		}
		[HttpPut]
		public async Task<IActionResult> UpdateStuffArchive(HashSet<StuffArchiveDto> archives)
		{
			return Ok(await _leaderServices.UpdateStuffArchive(archives));
		}
		[HttpGet("{campId}")]
		public async Task<IActionResult> DisplayNewRegister(int campId)
		{
			return Ok(await _leaderServices.DisplayNewRegisterAsync(campId));
		}

		[HttpPost]
		public async Task<IActionResult> SubmitNewRegisters(SubmitNewRegisterDto newRegisters)
		{
			return Ok(await _leaderServices.SubmitNewRegister(newRegisters));
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteNewRegister([FromBody] List<string> Ids)
		{
			if (Ids.IsNullOrEmpty())
			{
				throw new BadRequestException("Invalid reques");
			}

			return Ok(await _leaderServices.DeleteFromNewRegister(Ids));

		}
		[HttpGet]
		public async Task<IActionResult> CampRegisterStatus()
		{
			var response = new ServiceResponse<object>() { IsSuccess = true };

			response.Entity = await _unitOfWork.Camps.Get().Select(c => new
			{
				c.Name,
				state = c.OpenForRegister
			}).ToListAsync();

			return Ok(response);
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateCampStatus(int id)
		{
			return Ok(await _leaderServices.UpdateCampStatusAsync(id));
		}
		[HttpGet]
		public async Task<IActionResult> DisplayOpenedCamp()
		{
			var  response= new ServiceResponse<object>() { IsSuccess = true };
			response.Entity = _unitOfWork.Camps.findManyWithChildAsync(c => c.OpenForRegister).Result.Select(c => new
			{
				c.Id,
				c.Name
			});

			await Task.CompletedTask;

            return Ok(response);
		}
		[HttpPut]
		public async Task<IActionResult> UpdateCampHead(int headId, int campId, bool isAdd)
		{
			var response = new ServiceResponse<int>() { IsSuccess = true };

			HeadOfTraining head = await _unitOfWork.HeadofCamp.getByIdAsync(headId);

			if(head is null)
			{
				throw new KeyNotFoundException("Invalid Id");
			}

			if (isAdd)
			{
				head.CampId = campId;
			}
			else
			{
				await _unitOfWork.HeadofCamp.deleteAsync(head);
			}
			await _unitOfWork.completeAsync();

			return Ok(response);
		}
	}
}
