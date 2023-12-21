using ISC.Core.APIDtos;
using ISC.Core.Dtos;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Mvc;

namespace ISC.API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IAuthanticationServices _Auth;
		private readonly IPublicSerives _publicService;
		public AccountController(IAuthanticationServices auth, IPublicSerives publicService)
		{
			_Auth = auth;
			_publicService = publicService;
		}
		[HttpPost("NewTrainee")]
		public async Task<IActionResult> NewTraineeRegisteration([FromForm]NewRegisterationDto model)
		{
			return Ok(await _publicService.AddNewRegister(model));
		}
		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromForm] LoginDto user)
		{
			return Ok(await _Auth.loginAsync(user));
		}
	}
}
