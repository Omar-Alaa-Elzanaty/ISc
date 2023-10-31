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
using ISC.EF.Repositories;
using static ISC.Core.APIDtos.CodeForcesDtos;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using AutoMapper;

namespace ISC.API.Controllers
{
	[Route("api/[controller]/[action]")]
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
		private readonly ISheetServices _sheetServices;
		private readonly IMapper _mapper;
		public LeaderController(
			RoleManager<IdentityRole> roleManager,
			UserManager<UserAccount> userManager,
			IAuthanticationServices auth,
			IUnitOfWork unitofwork,
			IMailServices mailServices,
			ILeaderServices leaderServices,
			ISheetServices sheetServices,
			IMapper mapper)
		{
			_roleManager = roleManager;
			_userManager = userManager;
			_unitOfWork = unitofwork;
			_mailServices = mailServices;
			_auth = auth;
			_leaderServices = leaderServices;
			_sheetServices = sheetServices;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> DisplaySystemRoles()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			return Ok(roles.Select(role => role.Name));
		}

		[HttpGet]
		public async Task<IActionResult> DisplayStuff()
		{
			var Accounts = _userManager.Users.ToList();
			var TraineeAccounts = await _userManager.GetUsersInRoleAsync(Role.TRAINEE);
			var response = Accounts.Except(TraineeAccounts).Select(acc => new
			{
				acc.Id,
				FullName = acc.FirstName + ' ' + acc.MiddleName + ' ' + acc.LastName,
				acc.CodeForceHandle,
				acc.Email
			}).ToList();
			return Ok(response);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayTrainee()
		{
			var response = await _userManager.Users.Where(i =>
			 _userManager.GetRolesAsync(i).Result.Contains(Role.TRAINEE)).Select(acc => new
			 {
				 acc.Id,
				 acc.CodeForceHandle,
				 acc.Email,
				 acc.College,
				 CampName = _unitOfWork.Trainees.getCampofTrainee(acc.Id).Result.Name
			 }).ToListAsync();
			return Ok(response);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayAll()
		{

			var Accounts = await _userManager.Users.Select(i => new
			{
				i.Id,
				i.UserName,
				FullName = i.FirstName + ' ' + i.MiddleName + ' ' + i.LastName,
				Role = _userManager.GetRolesAsync(i),
				i.CodeForceHandle,
				i.Email,
				i.College,
				i.Gender,
				i.PhoneNumber
			}).ToListAsync();
			return Ok(Accounts);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayStuffWithoutHoc()
		{
			var HocUserId = _unitOfWork.HeadofCamp.getAllAsync().Result.Select(hoc => hoc.UserId).ToList();
			var StuffWithoutHoc = _userManager.Users.Where(user => HocUserId.Contains(user.Id) == false).ToList();
			return Ok(StuffWithoutHoc);
		}

		[HttpGet]
		public async Task<IActionResult> DisplayCamp()
		{
			return Ok(_unitOfWork.Camps.getAllAsync().Result);
		}

		[HttpPost]
		public async Task<IActionResult> Register([FromForm] RegisterDto newUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var model = await _auth.RegisterAsync(newUser);
			if (!model.IsAuthenticated)
			{
				return BadRequest(model.Message);
			}
			return Ok(new 
			{ 
				model.Token,
				model.ExpireOn,
				model.UserId
			});
		}

		[HttpPost]
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
					_ = ErrorList.Append(Role.Role + ',');
			}
			await _unitOfWork.completeAsync();
			if (ErrorList.Count != 0) {
				return BadRequest($"Can't save user to these roles{ErrorList}");
			}
			return Ok("Changes have been successfully");

		}

		[HttpDelete]
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

		[HttpDelete]
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

		[HttpPost]
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

		[HttpGet]
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

		[HttpPost]
		public async Task<IActionResult> AddToRole([FromBody] UserRoleDto users)
		{
			var response = await _leaderServices.AddToRoleAsync(users);
			if (!response.Success)
			{
				return Ok(response);
			}
			return Ok(response.Success);
		}
		[HttpPost]
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
		[HttpGet]
		public async Task<IActionResult> DisplayTraineeArchive()
		{
			var response = await _unitOfWork.TraineesArchive.getAllAsync();
			return Ok(response);
		}
		[HttpDelete]
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
		[HttpPut]
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
		[HttpGet]
		public async Task<IActionResult> DisplayStuffArchive()
		{
			return Ok(_unitOfWork.StuffArchive.getAllAsync().Result);
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteStuffArchive(List<string>members)
		{
			var archives = await _unitOfWork.StuffArchive.getAllAsync(sa => members.Contains(sa.NationalID));
			_unitOfWork.StuffArchive.deleteGroup(archives);
			_= await _unitOfWork.completeAsync();
			return Ok("Deleted successfully");
		}
		[HttpPut]
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
		[HttpGet]
		public async Task<IActionResult> DisplayNewRegister()
		{
			var response= await _leaderServices.DisplayNewRegisterAsync();
			if (!response.Success)
				return BadRequest("No entity");
			return Ok(response);
		}
		[HttpPost]
		public async Task<IActionResult> SubmitNewRegisters(SubmitNewRegisterDto newRegisters)
		{
			HashSet<string> refused = new HashSet<string>();
			foreach (var contest in newRegisters.ContestsInfo)
			{
				var standingResponse = await _sheetServices.SheetStanding(contest.ContestId, contest.IsSohag);
				if (!standingResponse.Success)
				{
					return BadRequest(standingResponse.Comment);
				}
				var statusResponse = await _sheetServices.SheetStatus(contest.ContestId, contest.IsSohag);
				if (!statusResponse.Success)
				{
					return BadRequest(statusResponse.Comment);
				}


				var memberPerProblem = statusResponse.Entity.GroupBy(submission =>
				new
				{
					Handle = submission.author.members.FirstOrDefault().handle,
					ProblemName = submission.problem.name
				}).Where(mps => mps.Any(sub => sub.verdict == "OK")).Select(mps => new
				{
					mps.Key.ProblemName,
					mps.Key.Handle
				}).GroupBy(mps => mps.Handle).Select(problemSolved => new
				{
					handle=problemSolved.Key,
					Count= problemSolved.Count()
				});

				float totalproblems = standingResponse.Entity.problems.Count();

				foreach( var member in memberPerProblem) {
					if (Math.Ceiling(member.Count / totalproblems) * 100.0 < contest.PassingPrecent)
					{
						refused.Add(member.handle);
					}
				}
			}

			var acceptedMember =await _unitOfWork.NewRegitseration
				.findManyWithChildAsync(nr => !refused.Contains(nr.CodeForceHandle)
										&& newRegisters.CandidatesNationalId.Contains(nr.NationalID) == true);

			var camp = _unitOfWork.Camps.getByIdAsync(newRegisters.CampId).Result.Name;

			List<Tuple<NewRegistration, AuthModel>> faillRegisteration = new List<Tuple<NewRegistration, AuthModel>>();
			List<NewRegistration> confirmedAcceptacne = new List<NewRegistration>();

			foreach (var member in acceptedMember)
			{
				var newTrainee = _mapper.Map<RegisterDto>(member);
				newTrainee.Roles.Add(Role.TRAINEE);
				newTrainee.CampId = newRegisters.CampId;

				var response =await _leaderServices.AutoMemberAddAsync(
					registerDto:newTrainee,
					campName:camp
					);
				if(response.Success) {
					if (response.Entity is not null)
						faillRegisteration.Add(new Tuple<NewRegistration, AuthModel>(member, response.Entity));
					else
						acceptedMember.Add(member);
				}
			}

			_unitOfWork.NewRegitseration.deleteGroup(acceptedMember);
			_ = _unitOfWork.NewRegitseration.DeleteAll();

			return Ok(faillRegisteration.Select(t => new
			{
				t.Item1.FirstName,
				t.Item1.MiddleName,
				t.Item1.LastName,
				t.Item1.Email,
				t.Item1.CodeForceHandle,
				t.Item2.UserName,
				t.Item2.Password
			}));
		}
	}
}
