﻿using ISC.API.APIDtos;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = Roles.LEADER)]
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
		[HttpPost("Register")]
		public async Task<IActionResult> registerAsync([FromForm] AdminRegisterDto newuser)
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
						$"Username: {newuser.UserName}\n" +
						$"Password: {newuser.Password}";
			bool Result=await _MailServices.sendEmailAsync(newuser.Email, "ICPC Sohag account validation",body);
			if (Result == false)
			{
				return BadRequest("There was error happened");
			}
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
				return BadRequest("some Data is required");
			}
			var Account=await _UserManager.FindByIdAsync(model.Id);
			if (Account == null)
			{
				return BadRequest("there is no Account with this properities!");
			}
			foreach(var Role in model.Roles)
			{
				bool Result=await new Roles(_UserManager, _UnitOfWork)
					.addToRoleAsync(Account, Role, new { model.MentorId, model.CampId });
				if (Result == false)
					return BadRequest("Can't save updates");
			}
			await _UnitOfWork.comleteAsync();
			return Ok("Changes have been successfully");
			
		}
		[HttpDelete("DeleteTrinees")]
		public async Task<IActionResult> deleteFromStuff(List<string>traineesid)
		{
			List<UserAccount> Trainees=new List<UserAccount>();
			foreach(string traineeid in traineesid)
			{
				await _UserManager.DeleteAsync(await _UserManager.FindByIdAsync(traineeid));
			}
			await _UnitOfWork.comleteAsync();
			return Ok($"removed\n{Trainees}");
		}
	}
}
