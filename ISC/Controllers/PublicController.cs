using CodeforceApiSerivces;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class PublicController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IOnlineJudgeServices _onlineJudgeServices;
		private readonly IUnitOfWork _UnitOfWork;
		public PublicController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork, IOnlineJudgeServices onlineJudgeServices)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
			_onlineJudgeServices = onlineJudgeServices;
		}
		[HttpGet]
		public async Task<IActionResult>getcontest(string contestid)
		{
			return Ok(_onlineJudgeServices.getContestStandingAsync(contestid,3,true));
		}
	}
}
