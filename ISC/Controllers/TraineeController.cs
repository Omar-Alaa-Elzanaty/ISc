using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Esf;
using Org.BouncyCastle.Bcpg;
using System.Security.Claims;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = $"{Role.TRAINEE}")]
	public class TraineeController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IOnlineJudgeServices _onlineJudgeServices;
		private readonly IUnitOfWork _UnitOfWork;
		public TraineeController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork, IOnlineJudgeServices onlineJudgeServices)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
			_onlineJudgeServices = onlineJudgeServices;
		}
		[HttpGet]
		public async Task<IActionResult>HaveComment()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if(userId is null)
			{
				return BadRequest(false);
			}

			var trainee =_UserManager.Users.Include(x => x.Trainee).FirstOrDefault(i=>i.Id==userId);
			if (trainee is null||trainee.Trainee is null)
			{
				return BadRequest(false);
			}

			var lastSessionId =  _UnitOfWork.Sessions
									.Get()
									.Where(s => s.Date < DateTime.Now)
									.OrderDescending()
									.FirstOrDefaultAsync()
									.Result?.Id;

			var attend = await _UnitOfWork.TraineesAttendence
									.Get()
									.AnyAsync(i => i.TraineeId == trainee.Trainee.Id);
			var hasComment = await _UnitOfWork.SessionsFeedbacks
									.Get()
									.AnyAsync(s => s.SessionId == lastSessionId && trainee.Trainee.Id == s.TraineeId);

			return Ok(attend == true && hasComment == false);
		}
	}
}
