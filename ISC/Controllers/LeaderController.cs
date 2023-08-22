
using CodeforceApiSerivces;
using ISC.API.APIDtos;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.EF.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = RolesTemplates.Leader)]
	public class LeaderController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		public LeaderController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager,IAuthanticationServices auth,IUnitOfWork unitofwork)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
		}
		[HttpPost("Register")]
		public async Task<IActionResult> register([FromForm] AdminRegisterDto newuser)
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

			return Ok(model);
		}
		[HttpPost("AddRole")]
		public async Task<IActionResult> addRole(string role)
		{

			var newRole = new IdentityRole(role);
			var result = await _RoleManager.CreateAsync(newRole);
			_UnitOfWork.comlete();
			return Ok(result.Succeeded);
		}
	}
}
