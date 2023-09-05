using ISC.API.APIDtos;
using ISC.API.ISerivces;
using ISC.Core.Models;
using MailKit.Security;
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
		private readonly IAuthanticationServices _Auth;
        public AccountController(IAuthanticationServices auth)
        {
            _Auth = auth;
        }
		[HttpPost("NewTrainee")]
		public async Task<IActionResult> newTraineeRegisteration()
		{
			throw new NotImplementedException();
		}
		[HttpPost("Login")]
		public async Task<IActionResult> loginAsync([FromForm] LoginDto user)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var result= await _Auth.loginAsync(user);
			return Ok(new
			{
				result.ExpireOn,
				result.Roles,
				result.Token

			});
		}
	}
}
