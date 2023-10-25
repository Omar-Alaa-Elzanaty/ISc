using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Core.APIDtos;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using ISC.Services.ISerivces.IModelServices;
using ISC.Core.Dtos;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles = Role.LEADER)]
	public class LeaderController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<UserAccount> _userManager;
		private readonly IAuthanticationServices _auth;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMailServices _mailServices;
		private readonly ILeaderServices _leaderServices;
		public LeaderController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork, IMailServices mailServices, ILeaderServices leaderServices)
		{
			_roleManager = roleManager;
			_userManager = userManager;
			_unitOfWork = unitofwork;
			_mailServices = mailServices;
			_auth = auth;
			_leaderServices = leaderServices;
		}
		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromForm] RegisterDto newUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var model = await _auth.RegisterAsync(newUser);
			if (!model.IsAuthenticated)
			{
				return BadRequest(model);
			}
			return Ok(model);
		}
		[HttpPost("AssignToStuffRoles")]
		public async Task<IActionResult> AssignToStuffRoles([FromBody] StuffNewRolesDto model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("some data is required");
			}
			var Account = await _userManager.FindByIdAsync(model.UserId);
			if (Account == null)
			{
				return BadRequest("there is no Account with these properities!");
			}
			List<string> ErrorList = new List<string>();
			foreach (var Role in model.UserRoles)
			{
				bool Result = await _unitOfWork.addToRoleAsync(Account, Role.Role, Role.CampId, Role.MentorId);
				if (Result == false)
					ErrorList.Append(Role.Role + ',');
			}
			await _unitOfWork.completeAsync();
			if (ErrorList.Count != 0) {
				return BadRequest($"Can't save user to these roles{ErrorList}");
			}
			return Ok("Changes have been successfully");

		}

		[HttpDelete("DeleteFromStuff")]
		public async Task<IActionResult> DeleteFromStuff(List<string> stuffusersId)
		{
			List<UserAccount> ErrorsList = new List<UserAccount>();
			foreach (string UserId in stuffusersId)
			{
				var Account = await _userManager.FindByIdAsync(UserId);
				var UserRoles = _userManager.GetRolesAsync(Account).Result.ToList();
				bool result = true;
				if (UserRoles.Contains(Role.MENTOR))
				{
					result = await _unitOfWork.Mentors.deleteAsync(UserId);
				}
				if (UserRoles.Contains(Role.HOC) && result == true)
				{
					result = await _unitOfWork.HeadofCamp.deleteEntityAsync(UserId);
				}
				if (result == true)
				{
					StuffArchive Archive = new StuffArchive()
					{
						FirstName = Account.FirstName,
						MiddleName = Account.MiddleName,
						LastName = Account.LastName,
						NationalID = Account.NationalId,
						BirthDate = Account.BirthDate,
						Grade = Account.Grade,
						College = Account.College,
						Gender = Account.Gender,
						CodeForceHandle = Account.CodeForceHandle,
						FacebookLink = Account.FacebookLink,
						VjudgeHandle = Account.VjudgeHandle,
						Email = Account.Email,
						PhoneNumber = Account.PhoneNumber
					};
					_unitOfWork.StuffArchive.addAsync(Archive);
					await _userManager.DeleteAsync(Account);
				}
				else
					ErrorsList.Append(Account);
			}
			await _unitOfWork.completeAsync();
			return Ok(ErrorsList);
		}
		[HttpDelete("DeleteFromTrainees")]
		public async Task<IActionResult> DeleteFromTrainees([FromBody] List<string> traineesIds)
		{
			var response = await _leaderServices.DeleteTraineesAsync(traineesIds);
			if (!response.Success)
			{
				return Ok(new
				{
					response.Success,
					response.Comment
				});
			}
			return Ok(response);
		}
		[HttpPost("AddCamp")]
		public async Task<IActionResult> AddCamp(CampDto camp)
		{
			var response = await _leaderServices.AddCampAsync(camp);
			if (!response.Success)
			{
				return BadRequest(new
				{
					response.Success,
					response.Comment
				});
			}
			return Ok(response);
		}

		[HttpGet("RoleUserDisplay")]
		public async Task<IActionResult> RoleUserDisplay()
		{
			var accounts = _userManager.Users.Select(i => new
			{
				i.Id,
				FullName = i.FirstName + ' ' + i.MiddleName + " " + i.LastName,
				i.CodeForceHandle,
				i.Email,
				i.College,
				i.Gender

			});
			return Ok(accounts);
		}

		[HttpPost("AddToRole")]
		public async Task<IActionResult> AddToRole([FromBody] UserRoleDto users)
		{
			var response = await _leaderServices.AddToRoleAsync(users);
			if (!response.Success)
			{
				return Ok(response);
			}
			return Ok(response.Success);
		}
		[HttpPost("AddRole")]
		public async Task<IActionResult> AddRole([FromBody]string role)
		{
			var result=await _roleManager.FindByNameAsync(role);
			if(result != null)
			{
				return BadRequest($"Role {role} is already exist!");
			}
			result = new IdentityRole() { Name = role };
			var response= await _roleManager.CreateAsync(result);
			if (!response.Succeeded)
			{
				return BadRequest(response.Errors);
			}

			return Ok("Add successful");
		}
		[HttpGet("DisplayTraineeArchive")]
		public async Task<IActionResult> DisplayTraineeArchive()
		{
			var response = await _unitOfWork.TraineesArchive.getAllAsync();
			return Ok(response);
		}
		[HttpDelete("DeleteTraineeArchive")]
		public async Task<IActionResult> DeleteTraineeArchive(List<string>members)
		{
			var trainees = await _unitOfWork.TraineesArchive.findManyWithChildAsync(ta => members.Contains(ta.NationalID));
			if (trainees == null)
			{
				return BadRequest("No account to remove");
			}
			_unitOfWork.TraineesArchive.deleteGroup(trainees);
			_= await _unitOfWork.completeAsync();
			return Ok("Deleted Successfully");
		}
		[HttpPut("UpdateTraineeArchive")]
		public async Task<IActionResult> UpdateTraineeArchive([FromBody]List<TraineeArchiveDto> archives)
		{
			var nationalIds = archives.Select(a => a.NationalId);
			var members =await _unitOfWork.TraineesArchive.findManyWithChildAsync(ta => nationalIds.Contains(ta.NationalID));
			foreach(var archive in archives)
			{
				var trainee = members.Single(m => m.NationalID == archive.NationalId);
				var name = archive.FullName.Split(' ');
				trainee.FirstName = name[0];
				trainee.MiddleName = name[1];
				trainee.LastName = name[2];
				trainee.College = archive.College;
				trainee.CodeForceHandle = archive.CodeforceHandle;
				trainee.FacebookLink = archive.FacebookLink;
				trainee.Email = archive.Email;
				trainee.VjudgeHandle = archive.VjudgeHandle;
				trainee.BirthDate = archive.BirthDate;
				trainee.Year = archive.Year;
				trainee.IsCompleted = archive.IsCompleted;
				trainee.PhoneNumber = archive.PhoneNumber;
			}
			_ = await _unitOfWork.completeAsync();
			return Ok("Success");
		}
		[HttpGet("DisplayStuffArchive")]
		public async Task<IActionResult> DisplayStuffArchive()
		{
			return Ok(_unitOfWork.StuffArchive.getAllAsync().Result);
		}
		[HttpDelete("DeleteStuffArchive")]
		public async Task<IActionResult> DeleteStuffArchive(List<string>members)
		{
			var archives = await _unitOfWork.StuffArchive.getAllAsync(sa => members.Contains(sa.NationalID));
			_unitOfWork.StuffArchive.deleteGroup(archives);
			_= await _unitOfWork.completeAsync();
			return Ok("Deleted successfully");
		}
		[HttpPut("UpdateStuffArchive")]
		public async Task<IActionResult> UpdateStuffArchive(List<StuffArchiveDto> archives)
		{
			var nationalIds=archives.Select(a=>a.NationalID).ToList();
			var members =await _unitOfWork.StuffArchive.findManyWithChildAsync(sa=>nationalIds.Contains(sa.NationalID));
			foreach (var stuffMember in archives) {
				var stuff = members.Single(m => m.NationalID == stuffMember.NationalID);
				var name = stuffMember.FullName.Split(' ');
				stuff.FirstName = name[0];
				stuff.MiddleName = name[1];
				stuff.LastName = name[2];
				stuff.NationalID= stuffMember.NationalID;
				stuff.BirthDate= stuffMember.BirthDate;
				stuff.Grade=stuffMember.Grade;
				stuff.College=stuffMember.College;
				stuff.Gender= stuffMember.Gender;
				stuff.CodeForceHandle = stuffMember.CodeForceHandle;
				stuff.FacebookLink= stuffMember.FacebookLink;
				stuff.VjudgeHandle = stuffMember.VjudgeHandle;
				stuff.Email = stuffMember.Email;
				stuff.PhoneNumber= stuffMember.PhoneNumber;
				stuff.Year=stuffMember.Year;
			}
			await _unitOfWork.completeAsync();
			return Ok("Update Successfully");
		}
	}
}
