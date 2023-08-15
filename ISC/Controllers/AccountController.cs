using ISC.API.APIDtos;
using ISC.API.Services;
using ISC.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IAuthanticationServices _auth;
        public AccountController(IAuthanticationServices auth)
        {
            _auth = auth;
        }
		[HttpPost("Register")]
		[Authorize(Roles ="Leader")]
		public async Task<IActionResult> register([FromForm]AdminRegisterDto newuser)
		{
			if(!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var model=await _auth.adminRegisterAsync(newuser);
			if(!model.IsAuthenticated)
			{
				return BadRequest(model);
			}

			return Ok(model);
		}
        [HttpGet("Login")]
		public async Task<IActionResult> loginAsync([FromForm]LoginDto user)
		{
			if(!ModelState.IsValid)
				return BadRequest(ModelState);
			var result= await _auth.loginAsync(user);
			return Ok(new
			{
				result.ExpireOn,
				result.Roles,
				result.Token

			});
		}
	}
}
