using ISC.API.APIDtos;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
			if (await _RoleManager.RoleExistsAsync(role))
				return BadRequest("Role is already exist!");
			var result = await _RoleManager.CreateAsync(newRole);
			await _UnitOfWork.comleteAsync();
			return Ok(result.Succeeded);
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
