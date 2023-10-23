using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Core.APIDtos;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using ISC.Services.ISerivces.IModelServices;
using ISC.Core.Dtos;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = Role.LEADER)]
	public class LeaderController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IMailServices _MailServices;
		private readonly ILeaderServices _LeaderServices;
		public LeaderController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork, IMailServices mailServices, ILeaderServices leaderServices)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_UnitOfWork = unitofwork;
			_MailServices = mailServices;
			_Auth = auth;
			_LeaderServices = leaderServices;
		}
		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromForm] RegisterDto newUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var model = await _Auth.RegisterAsync(newUser);
			if (!model.IsAuthenticated)
			{
				return BadRequest(model);
			}
			return Ok(model);
		}
		[HttpPost("AssignToStuffRoles")]
		public async Task<IActionResult> AssignToStuffRoles([FromBody] StuffNewRolesDto model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("some data is required");
			}
			var Account = await _UserManager.FindByIdAsync(model.UserId);
			if (Account == null)
			{
				return BadRequest("there is no Account with these properities!");
			}
			List<string> ErrorList = new List<string>();
			foreach (var Role in model.UserRoles)
			{
				bool Result = await _UnitOfWork.addToRoleAsync(Account, Role.Role, Role.CampId, Role.MentorId);
				if (Result == false)
					ErrorList.Append(Role.Role + ',');
			}
			await _UnitOfWork.comleteAsync();
			if (ErrorList.Count != 0) {
				return BadRequest($"Can't save user to these roles{ErrorList}");
			}
			return Ok("Changes have been successfully");

		}

		[HttpDelete("DeleteFromStuff")]
		public async Task<IActionResult> DeleteFromStuff(List<string> stuffusersId)
		{
			List<UserAccount> ErrorsList = new List<UserAccount>();
			foreach (string UserId in stuffusersId)
			{
				var Account = await _UserManager.FindByIdAsync(UserId);
				var UserRoles = _UserManager.GetRolesAsync(Account).Result.ToList();
				bool result = true;
				if (UserRoles.Contains(Role.MENTOR))
				{
					result = await _UnitOfWork.Mentors.deleteAsync(UserId);
				}
				if (UserRoles.Contains(Role.HOC) && result == true)
				{
					result = await _UnitOfWork.HeadofCamp.deleteEntityAsync(UserId);
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
					_UnitOfWork.StuffArchive.addAsync(Archive);
					await _UserManager.DeleteAsync(Account);
				}
				else
					ErrorsList.Append(Account);
			}
			await _UnitOfWork.comleteAsync();
			return Ok(ErrorsList);
		}
		[HttpDelete("DeleteFromTrainees")]
		public async Task<IActionResult> DeleteFromTrainees([FromBody] List<string> traineesIds)
		{
			var response = await _LeaderServices.DeleteTraineesAsync(traineesIds);
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
		[HttpPost("AddCamp")]
		public async Task<IActionResult> AddCamp(CampDto camp)
		{
			var response = await _LeaderServices.AddCampAsync(camp);
			if (!response.Success)
			{
				return BadRequest(new
				{
					response.Success,
					response.Comment
				});
			}
			return Ok(response);
		}

		[HttpGet("RoleUserDisplay")]
		public async Task<IActionResult> RoleUserDisplay()
		{
			var accounts = _UserManager.Users.Select(i => new
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

		[HttpPost("AddToRole")]
		public async Task<IActionResult> AddToRole([FromBody] UserRoleDto users)
		{
			var response = await _LeaderServices.AddToRoleAsync(users);
			if (!response.Success)
			{
				return Ok(response);
			}
			return Ok(response.Success);
		}
		[HttpPost("AddRole")]
		public async Task<IActionResult> AddRole([FromBody]string role)
		{
			var result=await _RoleManager.FindByNameAsync(role);
			if(result != null)
			{
				return BadRequest($"Role {role} is already exist!");
			}
			result = new IdentityRole() { Name = role };
			var response= await _RoleManager.CreateAsync(result);
			if (!response.Succeeded)
			{
				return BadRequest(response.Errors);
			}

			return Ok("Add successful");
		}
	}
}
