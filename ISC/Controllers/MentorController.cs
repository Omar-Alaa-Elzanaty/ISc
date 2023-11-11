using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace ISC.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	//[Authorize(Roles =$"{Roles.LEADER},{Roles.MENTOR},{Roles.HOC},{Roles.INSTRUCTOR}")]
	public class MentorController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IAuthanticationServices _Auth;
		private readonly IUnitOfWork _UnitOfWork;
		public MentorController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork)
		{
			_RoleManager = roleManager;
			_UserManager = userManager;
			_Auth = auth;
			_UnitOfWork = unitofwork;
		}
		[HttpGet]
		public async Task<IActionResult> DisplayOwnTrainees()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var mentor =await _UserManager.Users.Include(u => u.Mentor).SingleOrDefaultAsync(u => u.Id == userId);

			var trainees = _UserManager.Users.Include(u => u.Trainee).Include(u => u.Trainee.Camp)
				.Where(u => u.Trainee != null && u.Trainee.MentorId == mentor.Mentor.Id);

			return Ok(trainees.Select(t => new
			{
				FullName=t.FirstName+' '+t.MiddleName+' '+t.LastName,
				t.Email,
				t.PhoneNumber,
				FacebookPage=t.FacebookLink??" ",
				t.College,
				t.Gender,
				t.CodeForceHandle,
				t.Trainee.Camp.Name
			}));
		}

		[HttpGet]
		public async Task<IActionResult> DisplayTraineesProgress(int campId)
		{
			var mentorAccId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var mentor = await _UnitOfWork.Mentors.findByAsync(m => m.UserId == mentorAccId);

			var trainees = await _UserManager.Users
				.Include(u => u.Trainee)
				.Where(m => m.Trainee != null && m.Trainee.MentorId == mentor.Id && m.Trainee.CampId == campId).Select(t => new
				{
					t.Trainee.Id,
					FullName = t.FirstName + ' ' + t.MiddleName + ' ' + t.LastName,
				}).ToListAsync();

			if (trainees == null || trainees.Count() == 0) 
			{
				return BadRequest("There is no trainees");
			}

			var access = _UnitOfWork.TraineesSheetsAccess
				.findManyWithChildAsync(s => trainees.Any(t => t.Id == s.TraineeId), new[] {"Sheet"})
				.Result.ToList();

			var sheets = access.DistinctBy(a=>a.SheetId).Select(s => new
			{
				s.SheetId,
				s.Sheet.Name
			});

			var progress = new ProgressDto();

			foreach (var sheet in sheets)
			{
				progress.SheetsNames.Add(sheet.Name);

				foreach (var trainee in trainees)
				{
					var problemsCount = access
									.Single(i => i.TraineeId == trainee.Id && i.SheetId == sheet.SheetId).NumberOfProblems;
					var prog = progress.Trainees;
					if (prog.ContainsKey(trainee.Id))
					{
						prog[trainee.Id].solvedProblems.Add(problemsCount);
					}
					else
					{
						var traineeInfo = new TraineePrgoressDto() { FullName = trainee.FullName };
						traineeInfo.solvedProblems.Add(problemsCount);

						prog.TryAdd(trainee.Id, traineeInfo);
					}
				}
			}

			var TraineesInfo = progress.Trainees.Select(i => i.Value)?.ToList();
			var SheetsInfo = progress.SheetsNames;

			return Ok(new { SheetsInfo, TraineesInfo });
		}
		[HttpGet]
		public async Task<IActionResult>DisplayAttendence(int camp)
		{
			var mentorUserId = "8be8d356-96fa-4dc9-9a09-95222030def3";//User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var mentorId = _UnitOfWork.Mentors.findByAsync(m=>m.UserId==mentorUserId).Result.Id;
			var attendences = await _UnitOfWork.TraineesAttendence.getAllAsync();
			var traineesIds = attendences.DistinctBy(a => a.TraineeId).Select(a => a.TraineeId).ToList();
			var sessionIds=attendences.DistinctBy(a=>a.SessionId).Select(a=>a.SessionId).ToList();
			var trainees = await _UserManager.Users
								.Include(a => a.Trainee)
								.Where(a => a.Trainee != null && traineesIds.Contains(a.Trainee.Id)&&a.Trainee.MentorId==mentorId)
								.Select(a => new
								{
									a.Trainee.Id,
									FullName = a.FirstName + ' ' + a.MiddleName + ' ' + a.LastName
								})
								.ToListAsync();

			var sessions = _UnitOfWork.Sessions.findManyWithChildAsync(s => sessionIds.Contains(s.Id))
						.Result.OrderBy(s=>s.Date).Select(s => new
						{
							s.Id,
							s.Topic
						});
			var attendenceInfo =new TraineeAttendenceDto();
			attendenceInfo.Sessions.AddRange(sessions.Select(s => s.Topic).ToList());
			foreach(var trainee in trainees)
			{
				var traineeProgress = new TraineeInfoDto() { FullName = trainee.FullName };
				foreach(var session in sessions)
				{
					traineeProgress.IsAttend.Add(attendences.Any(a => a.TraineeId == trainee.Id && a.SessionId == session.Id));
				}
				attendenceInfo.Attendances.Add(traineeProgress);
			}
			return Ok(attendenceInfo);
		}
	}
}
