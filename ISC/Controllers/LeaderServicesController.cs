using ISC.API.ISerivces;
using ISC.Core.Interfaces;
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
	[Authorize(Roles =RolesTemplates.Leader)]
	public class LeaderServicesController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		public LeaderServicesController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
		}
		[HttpGet("MentorsDetails")]
		public async Task<IActionResult> showMentors()
		{
			return Ok(await _UnitOfWork.Mentors.showMentorsAccountsAsync());
		}
		[HttpGet("CampsDetails")]
		public async Task<IActionResult> showCamps()
		{
			return Ok(await _UnitOfWork.Camps.getAllAsync());
		}
	}
}
