using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.EF;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace ISC.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{Role.MENTOR}")]
	public class MentorController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<UserAccount> _userManager;
		private readonly IAuthanticationServices _auth;
		private readonly IUnitOfWork _unitOfWork;
		public MentorController(RoleManager<IdentityRole> roleManager, UserManager<UserAccount> userManager, IAuthanticationServices auth, IUnitOfWork unitofwork)
		{
			_roleManager = roleManager;
			_userManager = userManager;
			_auth = auth;
			_unitOfWork = unitofwork;
		}
		[HttpGet]
		public async Task<IActionResult> DisplayOwnTrainees()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var mentor = await _userManager.Users.Include(u => u.Mentor).SingleOrDefaultAsync(u => u.Id == userId);

			var trainees = _userManager.Users.Include(u => u.Trainee).Include(u => u.Trainee.Camp)
				.Where(u => u.Trainee != null && u.Trainee.MentorId == mentor.Mentor.Id);

			return Ok(trainees.Select(t => new
			{
				FullName = t.FirstName + ' ' + t.MiddleName + ' ' + t.LastName,
				t.Email,
				t.PhoneNumber,
				FacebookPage = t.FacebookLink ?? " ",
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
			var mentor = await _unitOfWork.Mentors.findByAsync(m => m.UserId == mentorAccId);

			var trainees = await _userManager.Users
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

			var access = _unitOfWork.TraineesSheetsAccess
				.findManyWithChildAsync(s => trainees.Any(t => t.Id == s.TraineeId), new[] { "Sheet" })
				.Result.ToList();

			var sheets = access.DistinctBy(a => a.SheetId).Select(s => new
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
		public async Task<IActionResult> DisplayAttendence(int camp)
		{
			var mentorUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var mentorId = _unitOfWork.Mentors.findByAsync(m => m.UserId == mentorUserId).Result.Id;
			var attendences = await _unitOfWork.TraineesAttendence.getAllAsync();
			var traineesIds = attendences.DistinctBy(a => a.TraineeId).Select(a => a.TraineeId).ToList();
			var sessionIds = attendences.DistinctBy(a => a.SessionId).Select(a => a.SessionId).ToList();
			var trainees = await _userManager.Users
								.Include(a => a.Trainee)
								.Where(a => a.Trainee != null && traineesIds.Contains(a.Trainee.Id) && a.Trainee.MentorId == mentorId)
								.Select(a => new
								{
									a.Trainee.Id,
									FullName = a.FirstName + ' ' + a.MiddleName + ' ' + a.LastName
								})
								.ToListAsync();

			var sessions = _unitOfWork.Sessions.findManyWithChildAsync(s => sessionIds.Contains(s.Id))
						.Result.OrderBy(s => s.Date).Select(s => new
						{
							s.Id,
							s.Topic
						});
			var attendenceInfo = new PersonAttendenceDto();
			attendenceInfo.Sessions.AddRange(sessions.Select(s => s.Topic).ToList());
			foreach (var trainee in trainees)
			{
				var traineeProgress = new PersonInfoDto() { FullName = trainee.FullName };
				foreach (var session in sessions)
				{
					traineeProgress.IsAttend.Add(attendences.Any(a => a.TraineeId == trainee.Id && a.SessionId == session.Id));
				}
				attendenceInfo.Attendances.Add(traineeProgress);
			}
			return Ok(attendenceInfo);
		}
		[HttpGet]
		public async Task<IActionResult> TakeAttendence()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var mentor = await _unitOfWork.Mentors.findByAsync(m => m.UserId == userId);

			if (mentor is null || mentor.AccessSessionId is null)
			{
				return BadRequest("Access denied");
			}

			var session = await _unitOfWork.Sessions.getByIdAsync((int)mentor.AccessSessionId);

			var trainees = await _unitOfWork.Trainees
											.Get()
											.Where(t => t.CampId == session.CampId)
											.Join(_userManager.Users,
											t => t.UserId
											, a => a.Id,
											(trainee, account) => new { account, trainee })
											.Select(g => new
											{
												FullName = g.account.FirstName + ' ' + g.account.MiddleName + ' ' + g.account.LastName,
												AccId = g.account.Id,
												TraineeId = g.trainee.Id
											})
											.ToListAsync();

			var traineeAttendence = await _unitOfWork.TraineesAttendence.findManyWithChildAsync(a => a.SessionId == session.Id);

			List<AttendenceDto> attendence = new List<AttendenceDto>();
			foreach (var trainee in trainees)
			{
				AttendenceDto attend = new AttendenceDto()
				{
					FullName = trainee.FullName,
					TraineeId = trainee.TraineeId
				};
				attend.IsAttend = traineeAttendence.Any(ta => ta.TraineeId == trainee.TraineeId);
				attendence.Add(attend);
			}

			return Ok(new { session.Id, attendence });
		}
		[HttpPut("{sessionId}")]
		public async Task<IActionResult> SubmitAttendence(int sessionId,List<SubmitAttendenceDto> attendence)
		{
			var session = await _unitOfWork.Sessions.getByIdAsync(sessionId);
			if(session is null)
			{
				return BadRequest("Invalid request");
			}
			var newAttendnce = new List<TraineeAttendence>();
			var absence = new List<TraineeAttendence>();
			foreach(var record in attendence)
			{
				var attend = await _unitOfWork.TraineesAttendence.findByAsync(a => a.SessionId == sessionId && a.TraineeId == record.TraineeId);
				var trainee = await _unitOfWork.Trainees.getByIdAsync(record.TraineeId);

				if (trainee is null)
					continue;

				if(attend  is not null && record.IsAttend == false)
				{
					absence.Add(attend);
				}
				else if(attend is null && record.IsAttend == true)
				{
					attend = new TraineeAttendence()
					{
						TraineeId = record.TraineeId,
						SessionId = sessionId
					};

					newAttendnce.Add(attend);
				}
			}
			_unitOfWork.TraineesAttendence.deleteGroup(absence);
			await _unitOfWork.TraineesAttendence.AddGroup(newAttendnce);
			_= await _unitOfWork.completeAsync();

			return Ok("update Attendence");
		}
		//TODO: to implement
		//[HttpGet]
		//public async Task<IActionResult> DisplayTraineeTask(int traineeId)
		//{
		//	return Ok();
		//}
		//[HttpDelete]
		//public async Task<IActionResult>DeleteTraineeTask(int taskId)
		//{
		//	return Ok();
		//}
		//[HttpPost]
		//public async Task<IActionResult>AddTask(int traineeId,List<string> tasks)
		//{
		//	return Ok();
		//}
		//[HttpPut]
		//public async Task<IActionResult>UpdateTask(int taskId, string task)
		//{
		//	return Ok();
		//}

	}
}
