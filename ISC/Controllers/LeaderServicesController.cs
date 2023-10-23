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
	//[Authorize(Roles = Roles.LEADER)]
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
		public async Task<IActionResult> DisplaySystemRoles()
		{
			var roles = await _RoleManager.Roles.ToListAsync();
			return Ok(roles.Select(role=>role.Name));
		}
		[HttpGet("DisplayStuff")]
		public  async Task<IActionResult> DisplayStuff()
		{
			var Accounts = _UserManager.Users.ToList();
			var TraineeAccounts =await _UserManager.GetUsersInRoleAsync(Role.TRAINEE);
			var response = Accounts.Except(TraineeAccounts).Select(acc => new
			{
				acc.Id,
				FullName=acc.FirstName+' '+acc.MiddleName+' '+acc.LastName,
				acc.CodeForceHandle,
				acc.Email
			}).ToList();
			return Ok(response);
		}
		[HttpGet("DisplayTrainee")]
		public async Task<IActionResult> DisplayTrainee()
		{
			var response =await _UserManager.Users.Where(i =>
			 _UserManager.GetRolesAsync(i).Result.Contains(Role.TRAINEE)).Select(acc => new
			 {
				 acc.Id,
				 acc.CodeForceHandle,
				 acc.Email,
				 acc.College,
				 CampName = _UnitOfWork.Trainees.getCampofTrainee(acc.Id)
			 }).ToListAsync();
			return Ok(response);
		}
		[HttpGet("DisplayAccounts")]
		public async Task<IActionResult> DisplayAll()
		{

			var Accounts = await _UserManager.Users.Select(i => new
			{
				i.Id,
				i.UserName,
				FullName=i.FirstName+' '+i.MiddleName+' '+i.LastName,
				Role=_UserManager.GetRolesAsync(i),
				i.CodeForceHandle,
				i.Email,
				i.College,
				i.Gender,
				i.PhoneNumber
			}).ToListAsync();
			return Ok(Accounts);
		}
		[HttpGet("DisplayStuffWithoutHoc")]
		public async Task<IActionResult> DisplayStuffWithoutHoc()
		{
			var HocUserId = _UnitOfWork.HeadofCamp.getAllAsync().Result.Select(hoc=>hoc.UserId).ToList();
			var StuffWithoutHoc = _UserManager.Users.Where(user=>HocUserId.Contains(user.Id)==false).ToList();
			return Ok(StuffWithoutHoc);
		}
		[HttpGet("DisplayNewRegister")]
		public async Task<IActionResult> DisplayNewRegister()
		{
			List<KeyValuePair<NewRegistration, string>> Filter=new List<KeyValuePair<NewRegistration, string>>();
			foreach(var newmember in await _UnitOfWork.NewRegitseration.getAllAsync())
			{
				if (await _UnitOfWork.TraineesArchive
			   .findByAsync(TA => (TA.NationalID == newmember.NationalID
							   || TA.CodeForceHandle == newmember.CodeForceHandle
							   || TA.Email == newmember.Email
							   ||(newmember.FacebookLink!=null&&newmember.FacebookLink==TA.FacebookLink)
							   ||(newmember.PhoneNumber!=null&&newmember.PhoneNumber==TA.PhoneNumber))
			   && TA.CampName.ToLower() == newmember.CampName.ToLower()) != null)
					Filter.Add(new(newmember, "Archive"));
				else Filter.Add(new(newmember, "New"));
			}
			return Ok(Filter);
		}
		[HttpGet("DisplayCamp")]
		public async Task<IActionResult> DisplayCamp()
		{
			return Ok( _UnitOfWork.Camps.getAllAsync().Result);
		}
	}
}
