using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.EF;
using ISC.EF.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles =RolesTemplates.Leader)]
	public class LeaderServicesController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IUnitOfWork _UnitOfWork;
		public LeaderServicesController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IUnitOfWork unitofwork)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_UnitOfWork = unitofwork;
		}
		
		
		[HttpGet("AvailableRoles")]
		public async Task<IActionResult> displaySystemRoles()
		{
			return Ok(_RoleManager.Roles.ToList().Select(role=>role.Name));
		}
		[HttpGet("ShowStuff")]
		public  async Task<IActionResult> displayStuff()
		{
			var Accounts = _UserManager.Users.ToList();
			var TraineeAccounts =await _UserManager.GetUsersInRoleAsync(RolesTemplates.Trainee);
			return Ok(Accounts.Except(TraineeAccounts));
		}
	}
}
