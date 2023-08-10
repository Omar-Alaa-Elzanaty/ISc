using ISC.API.Dtos;
using ISC.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		private readonly UserManager<UserAccount> _userManager;
        public LoginController(UserManager<UserAccount>userManager)
        {
            _userManager= userManager;
        }
        [HttpPost]
		public async Task<IActionResult> Register(Class user)
		{
			UserAccount account=new UserAccount();
			account.UserName=user.UserName;
			account.FirstName=user.FirstName;
			account.MiddleName=user.MiddleName;
			account.LastName=user.LastName;
			account.Email=user.Email;
			account.NationalId=user.NationalId;
			account.BirthDate =DateTime.Parse(user.BirthDate);
			account.Gender=user.Gender;
			account.Grade=user.Grade;
			account.CodeForceHandle=user.CodeForceHandle;
			account.PhoneNumber = user.phone;
			account.College=user.College;
			var result=await _userManager.CreateAsync(account,user.Password);
			if(result.Succeeded)
			{
				return Ok(user);
			}
			else
			{
				return BadRequest(result);
			}
		}
	}
}
