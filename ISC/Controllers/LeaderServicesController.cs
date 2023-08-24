using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = Roles.LEADER)]
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
		
		
		[HttpGet("DisplayRoles")]
		public async Task<IActionResult> displaySystemRoles()
		{
			var roles = await _RoleManager.Roles.ToListAsync();
			return Ok(roles.Select(role=>role.Name));
		}
		[HttpGet("DisplayStuff")]
		public  async Task<IActionResult> displayStuff()
		{
			var Accounts = _UserManager.Users.ToList();
			var TraineeAccounts =await _UserManager.GetUsersInRoleAsync(Roles.TRAINEE);
			return Ok(Accounts.Except(TraineeAccounts));
		}
	}
}
