using ISC.API.APIDtos;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection.Metadata.Ecma335;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles = Roles.LEADER)]
	public class LeaderController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IMailServices _MailServices;
		public LeaderController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager,IAuthanticationServices auth,IUnitOfWork unitofwork,IMailServices mailServices)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_UnitOfWork = unitofwork;
			_MailServices = mailServices;
			_Auth = auth;
		}
		[HttpPost("RegisterNewUser")]
		public async Task<IActionResult> registerAsync([FromForm] RegisterDto newuser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var model = await _Auth.adminRegisterAsync(newuser);
			if (!model.IsAuthenticated)
			{
				return BadRequest(model);
			}
			string body = "We need to inform you that your account on ISc being ready to use\n" +
						"this is your creadntial informations\n" +
						$"Username: {model.UserName}\n" +
						$"\nPassword: {model.Password}";
			//bool Result = await _MailServices.sendEmailAsync(newuser.Email, "ICPC Sohag account", body);
			//if (Result == false)
			//{
			//	return BadRequest("Email is not valid");
			//}
			return Ok(model);
		}
		[HttpPost("AddRole")]
		public async Task<IActionResult> addRole(string role)
		{
			var newRole = new IdentityRole(role);
			if (await _RoleManager.RoleExistsAsync(role))
			{
				return BadRequest("Role is already exist!");
			}
			var result = await _RoleManager.CreateAsync(newRole);
			await _UnitOfWork.comleteAsync();
			return Ok(result.Succeeded);
		}
		[HttpPost("AddMissions")]
		public async Task<IActionResult> assignToStuffRoles(StuffNewRolesDto model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("some data is required");
			}
			var Account=await _UserManager.FindByIdAsync(model.UserId);
			if (Account == null)
			{
				return BadRequest("there is no Account with these properities!");
			}
			List<string>ErrorList = new List<string>();
			foreach(var Role in model.UserRoles)
			{
				bool Result = await _UnitOfWork.addToRoleAsync(Account, Role.Role, Role.CampId, Role.MentorId);
				if (Result == false)
					ErrorList.Append(Role.Role+',');
			}
			await _UnitOfWork.comleteAsync();
			if(ErrorList.Count != 0) {
				return BadRequest($"Can't save User to these roles{ErrorList}");
			}
			return Ok("Changes have been successfully");
			
		}
		[HttpDelete("DeleteFromTrainees")]
		public async Task<IActionResult> deleteFromTrainees(List<string>traineesusersid)
		{
			List<UserAccount> Trainees=new List<UserAccount>();
			foreach(string traineeuserid in traineesusersid)
			{
				await _UserManager.DeleteAsync(await _UserManager.FindByIdAsync(traineeuserid));
			}
			await _UnitOfWork.comleteAsync();
			return Ok($"removed\n{Trainees}");
		}
		[HttpDelete("DeleteFromStuff")]
		public async Task<IActionResult> deleteFromStuff(List<string> stuffusersid)
		{
			List<UserAccount>ErrorsList=new List<UserAccount>();
			foreach(string UserId in stuffusersid)
			{
				var Account = await _UserManager.FindByIdAsync(UserId);
				var UserRoles = _UserManager.GetRolesAsync(Account).Result.ToList();
				bool result = true;
				if (UserRoles.Contains(Roles.MENTOR))
				{
					result =await _UnitOfWork.Mentors.deleteEntityAsync(UserId);
				}
				if(UserRoles.Contains(Roles.HOC)&&result==true)
				{
					result = await _UnitOfWork.HeadofCamp.deleteEntityAsync(UserId);
				}
				if (result == true)
					await _UserManager.DeleteAsync(Account);
				else
					ErrorsList.Append(Account);
			}
			await _UnitOfWork.comleteAsync();
			return Ok(ErrorsList);
		}
	}
}
