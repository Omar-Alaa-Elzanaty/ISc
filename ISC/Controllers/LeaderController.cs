using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Leader")]
	public class LeaderController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;
        public LeaderController(RoleManager<IdentityRole> roleManager)
        {
			_roleManager = roleManager;
        }
        [HttpPost("AddRole")]
		public async Task<IActionResult> addRole(string role)
		{

			var newRole = new IdentityRole(role);
			var result = await _roleManager.CreateAsync(newRole);
			return Ok(result.Succeeded);
		}
	}
}
