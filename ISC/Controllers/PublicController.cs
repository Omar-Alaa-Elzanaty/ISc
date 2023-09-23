using ISC.API.ISerivces;
using ISC.API.Services;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Esf;
using System.Security.Claims;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = $"{Roles.TRAINEE}")]
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
			setLastRegister();
			//return Ok(await _onlineJudgeServices.getContestStandingAsync(contestid,50,true));
			//return Ok(await _onlineJudgeServices.getContestStatus(contestid));
			//return Ok(await _onlineJudgeServices.getUserStatusAsync());
			//return Ok(await new ScheduleSerives(_UnitOfWork, _onlineJudgeServices, _UserManager).updateTraineeSolveCurrentAccessAsync());
			return Ok();
		}
		[ApiExplorerSettings(IgnoreApi =true)]
		public async void setLastRegister()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var TraineeAccount = await _UserManager.FindByIdAsync(userId);
			if (TraineeAccount != null)
			{
				TraineeAccount.LastLoginDate = DateTime.Now;
				await _UnitOfWork.comleteAsync();
			}
		}
	}
}
