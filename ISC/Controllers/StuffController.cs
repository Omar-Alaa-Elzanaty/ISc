using ISC.Core.Interfaces;
using ISC.EF;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles =$"{Roles.LEADER},{Roles.MENTOR},{Roles.HOC},{Roles.INSTRUCTOR}")]
	public class StuffController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		public StuffController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
		}
		[HttpGet("DisplayCamps")]
		public async Task<IActionResult> displayCamps()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return Ok(await _UnitOfWork.Camps.getAllAsync());
		}

		[HttpGet("DisplayMentors")]
		public async Task<IActionResult> displayMentors()
		{
			return Ok(await _UnitOfWork.Mentors.showMentorsAccountsAsync());
		}
	}
}
