using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Core.APIDtos;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ISC.Services.ISerivces.IModelServices;
using ISC.Core.ModelsDtos;
using ISC.Core.Dtos;
using ISC.EF.Repositories;
using AutoMapper;
using System.Net;
using ISC.Services.Services.ModelSerivces;

namespace ISC.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	//[Authorize(Roles =$"{Roles.LEADER},{Roles.HOC}")]
	public class HeadCampController : ControllerBase
	{
		private readonly UserManager<UserAccount> _userManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMailServices _MailService;
		private readonly IOnlineJudgeServices _onlineJudgeSrvices;
		private readonly ISheetServices _sheetServices;
		private readonly ISessionsServices _sessionsSrvices;
		private readonly IHeadSerivces _headServices;
		private readonly IMapper _mapper;
		public HeadCampController(UserManager<UserAccount> userManager,
			IUnitOfWork unitofwork,
			IOnlineJudgeServices onlinejudgeservices,
			IMailServices mailService,
			ISheetServices sheetServices,
			IHeadSerivces headServices,
			IMapper mapper)
		{
			_userManager = userManager;
			_unitOfWork = unitofwork;
			_onlineJudgeSrvices = onlinejudgeservices;
			_MailService = mailService;
			_sheetServices = sheetServices;
			_headServices = headServices;
			_mapper = mapper;
		}
		[HttpGet]
		public async Task<IActionResult> DisplayTrainees()
		{
			return Ok(_userManager.GetUsersInRoleAsync(Role.TRAINEE).Result.Select(tr => new
			{
				tr.Id,
				FullName = tr.FirstName + ' ' + tr.MiddleName + ' ' + tr.LastName,
				tr.Email,
				tr.CodeForceHandle,
				tr.Gender,
				tr.Grade
			}));
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteFromTrainees([FromBody] List<string> traineesUsersId)
		{
			foreach (string traineeuserid in traineesUsersId)
			{
				var TraineeAccount = await _userManager.Users.Include(i => i.Trainee).Where(user => user.Id == traineeuserid).SingleOrDefaultAsync();
				if (TraineeAccount != null)
				{
					var Camp = await _unitOfWork.Trainees.getCampofTrainee(TraineeAccount.Trainee.Id);
					TraineeArchive Archive = new TraineeArchive()
					{
						FirstName = TraineeAccount.FirstName,
						MiddleName = TraineeAccount.MiddleName,
						LastName = TraineeAccount.LastName,
						NationalID = TraineeAccount.NationalId,
						BirthDate = TraineeAccount.BirthDate,
						Grade = TraineeAccount.Grade,
						Gender = TraineeAccount.Gender,
						College = TraineeAccount.College,
						CodeForceHandle = TraineeAccount.CodeForceHandle,
						FacebookLink = TraineeAccount.FacebookLink,
						VjudgeHandle = TraineeAccount.VjudgeHandle,
						Email = TraineeAccount.Email,
						PhoneNumber = TraineeAccount.PhoneNumber,
						CampName = Camp.Name,
						IsCompleted = false
					};
					await _unitOfWork.TraineesArchive.addAsync(Archive);
					await _userManager.DeleteAsync(TraineeAccount);
				}
			}
			await _unitOfWork.completeAsync();
			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> WeeklyFilter([FromBody] List<string> selectedTrainee)
		{
			string? headOfCampUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			Camp? camp = _unitOfWork.HeadofCamp.findWithChildAsync(t => t.UserId == headOfCampUserId,
																new[] { "Camp", })?.Result?.Camp ?? null;
			var traineesId = await _unitOfWork.Trainees
							.Get()
							.Where(t => selectedTrainee.Contains(t.UserId)).Select(t => t.Id)
							.ToListAsync();

			var result = await _sheetServices.TraineeSheetAccesWithout(traineesId, camp?.Id ?? 0);
			if (result.Success == false)
			{
				return BadRequest(result.Comment);
			}
			List<TraineeSheetAccess> traineesAccess = result.Entity;
			var ProblemSheetCount = _sheetServices.TraineeSheetProblemsCount(traineesAccess).Result.Entity;
			var FilteredOnSheets = _sheetServices.TraineesFilter(traineesAccess, ProblemSheetCount).Result.Entity;
			var traineesIds = traineesAccess.Select(i => i.TraineeId).ToList();
			var FilteredOnSessions = _sessionsSrvices.SessionFilter(traineesIds).Result.Entity;
			List<KeyValuePair<FilteredUserDto, string>> Filtered = new List<KeyValuePair<FilteredUserDto, string>>();
			int tsaSize = traineesAccess.Count();
			for (int Trainee = 0; Trainee < tsaSize; Trainee++)
			{
				bool FoundInSheetFilter = FilteredOnSheets?.Contains(traineesAccess[Trainee].TraineeId) ?? false;
				bool FoundInSessionFilter = FilteredOnSessions?.Contains(traineesAccess[Trainee].TraineeId) ?? false;
				if (FoundInSheetFilter || FoundInSessionFilter)
				{
					UserAccount TraineeAccount = await _userManager.FindByIdAsync(traineesAccess[Trainee].Trainee.UserId);
					if (TraineeAccount != null)
					{
						var FilteredUser = new FilteredUserDto()
						{
							UserId = TraineeAccount.Id,
							FirstName = TraineeAccount.FirstName,
							MiddleName = TraineeAccount.MiddleName,
							LastName = TraineeAccount.LastName,
							Email = TraineeAccount.Email,
							PhoneNumber = TraineeAccount.PhoneNumber,
							CodeforceHandle = TraineeAccount.CodeForceHandle,
							College = TraineeAccount.College,
							Gender = TraineeAccount.Gender,
							Grade = TraineeAccount.Grade
						};
						StringBuilder Reason = new StringBuilder();
						if (FoundInSheetFilter == true)
						{
							Reason.Append("Sheets");
						}
						if (FoundInSessionFilter == true)
						{
							Reason.Append(Reason.Length != 0 ? "/Sessions" : "Sessions");
						}
						Filtered.Add(new(FilteredUser, Reason.ToString()));
					}
				}
			}
			return Ok(Filtered);
		}
		[HttpDelete]
		public async Task<IActionResult> SubmitWeeklyFilter([FromBody] List<string> usersid)
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			//Camp Camp = await _UnitOfWork.Camps.getCampByUserIdAsync(userId);
			Camp? Camp = _unitOfWork.HeadofCamp.findWithChildAsync(t => t.UserId == userId,
																new[] { "Camp", }).Result?.Camp ?? null;
			List<UserAccount> Fail = new List<UserAccount>();
			foreach (var Id in usersid)
			{
				UserAccount traineeAccount = await _userManager.FindByIdAsync(Id);
				if (traineeAccount != null)
				{
					TraineeArchive ToArchive = new TraineeArchive()
					{
						FirstName = traineeAccount.FirstName,
						MiddleName = traineeAccount.MiddleName,
						LastName = traineeAccount.LastName,
						NationalID = traineeAccount.NationalId,
						BirthDate = traineeAccount.BirthDate,
						Grade = traineeAccount.Grade,
						College = traineeAccount.College,
						Gender = traineeAccount.Gender,
						CodeForceHandle = traineeAccount.CodeForceHandle,
						FacebookLink = traineeAccount.FacebookLink,
						VjudgeHandle = traineeAccount.VjudgeHandle,
						Email = traineeAccount.Email,
						PhoneNumber = traineeAccount.PhoneNumber,
						Year = Camp.Year,
						CampName = Camp.Name,
						IsCompleted = false
					};
					var Result = await _MailService.sendEmailAsync(traineeAccount.Email, "ICPC Sohag Filteration announcement"
						, $"Hello {traineeAccount.FirstName} + ' ' + {traineeAccount.MiddleName},{@"<\br>"} We regret to inform you that we had to remove you from the {Camp.Name} training program." +
						$" If you're interested in exploring other training programs, please let us know, and we'll provide you with more information." +
						$" Thank you for your efforts, and we hope you'll take this as a learning experience to continue your growth and development." +
						$"{@"<\br>"}{@"<\br>"}Best regards,{@"<\br>"}{@"<\br>"} ISc System{@"<\br>"}{@"<\br>"} Omar Alaa");
					if (Result == true)
					{
						await _unitOfWork.TraineesArchive.addAsync(ToArchive);
						await _userManager.DeleteAsync(traineeAccount);
					}
					else
					{
						Fail.Add(traineeAccount);
					}
				}
			}
			await _unitOfWork.completeAsync();
			return Ok(new { Fail, Comment = Fail.IsNullOrEmpty() ? "" : "please back to system Admin to solve this problem" });
		}
		[HttpGet]
		public async Task<IActionResult> DisplayMentors()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest("Invalid Request");
			}
			var camp = _unitOfWork.HeadofCamp
				.Get()
				.Include(h => h.Camp)
				.FirstOrDefaultAsync(h => h.UserId == userId).Result?.Camp;

			List<object> mentors = new List<object>();
			var mentorsOfCamp = await _unitOfWork.Camps.findWithChildAsync(c => c.Id == camp.Id, new[] { "Mentors" });

			foreach (var member in mentorsOfCamp?.Mentors)
			{
				UserAccount userInfo = await _userManager.FindByIdAsync(member.UserId);
				mentors.Add(new
				{
					member.Id,
					member.UserId,
					FullName = userInfo.FirstName + ' ' + userInfo.MiddleName + " " + userInfo.LastName,
				});
			}
			return Ok(mentors);
		}
		[HttpGet]
		public async Task<IActionResult> DisplayMentorTrainee()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return BadRequest("Invalid Request");
			}

			return Ok(await _headServices.DisplayTraineeMentorAsync(userId));
		}
		[HttpPost]
		public async Task<IActionResult> SubmitTraineesMentors([FromBody] List<AssignTraineeMentorDto> data)
		{
			await _headServices.SubmitTraineeMentorAsync(data);
			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> DisplaySessions()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var hoc = await _unitOfWork.HeadofCamp.GetByUserId(userId);

			if (hoc == null)
			{
				return BadRequest("Error in account");
			}

			var sessions = await _unitOfWork.Sessions.findManyWithChildAsync(s => s.CampId == hoc.CampId);

			return Ok(sessions);
		}
		[HttpPost]
		public async Task<IActionResult> AddSession([FromBody] SessionDto model)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			if (campId is null)
			{
				return BadRequest("no camp found");
			}

			model.Topic = model.Topic.ToLower();
			model.InstructorName = model.InstructorName.ToLower();

			var isFound = await _unitOfWork.Sessions
				.Get()
				.AnyAsync(s => !((s.Topic == model.Topic && s.CampId == campId) ||
									(s.Date.Day == model.Date.Day && s.Date.Month == model.Date.Month ||
										(campId == model.CampId || s.InstructorName == model.InstructorName))));

			if (!isFound)
			{
				return BadRequest("this session may record before or Conflict with other session");
			}

			Session session = _mapper.Map<Session>(model);
			session.CampId = (int)campId;

			await _unitOfWork.Sessions.addAsync(session);
			_ = await _unitOfWork.completeAsync();

			return Ok();
		}
		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveSession(int id)
		{
			var session = await _unitOfWork.Sessions.getByIdAsync(id);

			if (session is null)
			{
				return BadRequest("no session found");
			}

			_ = await _unitOfWork.Sessions.deleteAsync(session);
			_ = await _unitOfWork.completeAsync();

			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> DisplayInstructor()
		{
			return Ok(_userManager.GetUsersInRoleAsync(Role.INSTRUCTOR).Result.Select(i => i.FirstName + ' ' + i.MiddleName));
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSessionInfo([FromBody] SessionDto model, int id)
		{
			var session = await _unitOfWork.Sessions.getByIdAsync(id);

			if (session is null)
			{
				return NotFound("Invalid session");
			}
			var validResponse = await _unitOfWork.Sessions.CheckUpdateAbility(session, model, id);
			if (!validResponse.Success)
			{
				return BadRequest(validResponse.Comment);
			}

			session.Topic = model.Topic;
			session.InstructorName = model.InstructorName;
			session.LocationLink = model.LocationLink;
			session.LocationName = model.LocationName;
			session.Date = model.Date;

			await _unitOfWork.Sessions.UpdateAsync(session);
			_ = await _unitOfWork.completeAsync();

			return Ok();
		}
		[HttpGet()]
		public async Task<IActionResult> DisplaySheet()
		{
			var userId = "5f00d005-aafb-4bd9-8341-68a4cf2f8a22";// User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;
			if(campId is null)
			{
				return NotFound("Invalid account");
			}
			var sheets = _unitOfWork.Sheets
				.Get()
				.Where(s => s.CampId == campId)
				.OrderBy(s=>s.SheetOrder)
				.Select(s => new
				{
					s.Id,
					s.Name,
					s.SheetLink,
					s.SheetCfId,
					s.IsSohag,
					s.MinimumPrecent,
					s.SheetOrder,
				});
			return Ok(sheets);
		}
		[HttpPost]
		public async Task<IActionResult> AddSheet([FromBody] SheetDto model)
		{
			var userId = "5f00d005-aafb-4bd9-8341-68a4cf2f8a22";// User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			model.Name = model.Name.ToLower();

			var isValidToAdd = await _unitOfWork.Sheets.isValidToAddAsync(model,campId);

			if(!isValidToAdd)
			{
				return BadRequest("Conflict found");
			}

			var sheet=_mapper.Map<Sheet>(model);
			sheet.CampId = (int)campId;

			await _unitOfWork.Sheets.addAsync(sheet);
			_= await _unitOfWork.completeAsync();

			return Ok("Success");
		}
		[HttpDelete("{id}")]
		public async Task<IActionResult> RemoveSheet(int id)
		{
			var Sheet = await _unitOfWork.Sheets.getByIdAsync(id);

			if(Sheet is null)
			{
				return NotFound("Couldn't delete");
			}

			if(!await _unitOfWork.Sheets.deleteAsync(Sheet))
			{
				return BadRequest("Couldn't delete");
			}

			_unitOfWork.completeAsync().Wait();

			return Ok();
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSheets(int id, [FromBody] SheetDto model)
		{
			var userId = "5f00d005-aafb-4bd9-8341-68a4cf2f8a22";// User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			var sheet = await _unitOfWork.Sheets.getByIdAsync(id);

			if (sheet is null||!await _unitOfWork.Sheets.isValidToUpdateAsync(model,campId,id))
			{
				return NotFound("Couldn't update");
			}
			sheet.Name = model.Name;
			sheet.SheetLink = model.SheetLink;
			sheet.IsSohag=model.IsSohag;
			sheet.SheetCfId=model.SheetCfId;
			sheet.MinimumPrecent=model.MinimumPrecent;
			sheet.SheetOrder=model.SheetOrder;

			await _unitOfWork.Sheets.UpdateAsync(sheet);
			_= await _unitOfWork.completeAsync();

			return Ok("Success");
		}
		[HttpGet("{sheetId}")]
		public async Task<IActionResult> DisplayMaterials(int sheetId)
		{
			var materials = await _unitOfWork.Materials.findManyWithChildAsync(m => m.SheetId.Equals(sheetId));

			return Ok(materials.OrderBy(m=>m.CreationDate).Select(m => new
			{
				m.Id,
				m.Name,
				m.Link
			}).ToList());
		}
		[HttpPost]
		public async Task<IActionResult>AddMaterial(int sheetId,MaterialDto model)
		{
			if(await _unitOfWork.Sheets.getByIdAsync(sheetId) == null)
			{
				return BadRequest("Invalid request");
			}

			if (await _unitOfWork.Materials.Get().AnyAsync(m=>(m.Name==model.Name||m.Link==model.Link)&&m.SheetId==sheetId))
			{
				return BadRequest("Data conflict");
			}

			Material material=_mapper.Map<Material>(model);
			material.SheetId = sheetId;

			await _unitOfWork.Materials.addAsync(material);
			_ = await _unitOfWork.completeAsync();

			return Ok("Success");
		}
		[HttpDelete("{materialId}")]
		public async Task<IActionResult>RemoveMaterial(int materialId)
		{
			var material = await _unitOfWork.Materials.getByIdAsync(materialId);

			if (material == null)
			{
				return NotFound("Invalid Request");
			}

			await _unitOfWork.Materials.deleteAsync(material);
			_ = await _unitOfWork.completeAsync();

			return Ok();
		}
		[HttpPut("{materialId}")]
		public async Task<IActionResult>UpdateMaterial(int materialId,MaterialDto model)
		{
			var material = await _unitOfWork.Materials.getByIdAsync(materialId);

			var isValidToUpdate = !await _unitOfWork.Materials.Get()
										.AnyAsync(m => (m.Name == model.Name || m.Link == model.Link) && m.SheetId != material.SheetId);
			if (!isValidToUpdate)
			{
				return BadRequest("Conflict Update");
			}

			material.Name = model.Name;
			material.Link = model.Link;

			await _unitOfWork.Materials.UpdateAsync(material);
			_ = await _unitOfWork.completeAsync();

			return Ok("Success");
		}
		[HttpGet]
		public async Task<IActionResult> DisplaySheetAccess()
		{
			var userId = "5f00d005-aafb-4bd9-8341-68a4cf2f8a22";// User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var sheets =  _unitOfWork.HeadofCamp.Get().Include(h => h.Camp)?.Include(h => h.Camp.Sheets)
				.FirstOrDefault(h => h.Camp != null && h.UserId == userId)?.Camp?.Sheets.Select(i => new
				{
					i.Id,
					i.Name
				}).ToList();

			var sheetAccess = _unitOfWork.TraineesSheetsAccess.findManyWithChildAsync(ac => sheets.Any(s => s.Id == ac.SheetId))
								.Result.Select(s=>s.SheetId).ToList();

			var access = new List<SheetAccessStatusDto>();

			foreach(var sheet in sheets)
			{
				var sheetStatus = new SheetAccessStatusDto()
				{
					SheetId = sheet.Id,
					Name = sheet.Name,
					IsReachAble = sheetAccess.Contains(sheet.Id)
				};
				access.Add(sheetStatus);
			}

			return Ok(access);
		}
		[HttpGet]
		public async Task<IActionResult> DisplayTraineeSheetAccess()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest("Invalid request");
			}
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			return Ok(await _headServices.DisplayTraineeAccess(campId ?? 0));
			
		}
		[HttpPost("{sheetId}")]
		public async Task<IActionResult> AddNewAccess(int sheetId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return BadRequest("Invalid request");
			}
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			await _headServices.AddNewTrainingSheetAccess(sheetId, campId ?? 0);

			return Ok("Success");
		}
		[HttpPut("{sheetId}/{traineeId}")]
		public async Task<IActionResult> UpdateTraineeAccess(int sheetId,int traineeId)
		{
			await _unitOfWork.TraineesSheetsAccess.addAsync(new TraineeSheetAccess() { TraineeId= traineeId,SheetId=sheetId });
			_ = await _unitOfWork.completeAsync();
			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> AttendenceAccessPage()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return BadRequest("Invalid request");
			}
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			var availableMentors = await _userManager.Users.Include(u => u.Mentor)
									.Where(u => u.Mentor != null).Select(u => new
									{
										u.Mentor.Id,
										FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName,
									}).ToListAsync();

			var sessions = await _unitOfWork.Sessions.Get()
							.Where(s=>s.CampId==campId)
							.Select(s => new
							{
								s.Id,
								s.Topic
							}).ToListAsync();

			return Ok(new { availableMentors, sessions });
		}
		[HttpPut]
		public async Task<IActionResult> GiveAttendenceAccess(int mentorId, int sessionId)
		{
			var mentor = await _unitOfWork.Mentors.getByIdAsync(mentorId);
			var session = await _unitOfWork.Sessions.getByIdAsync(sessionId);

			if (mentor is null || session is null)
			{
				return BadRequest("Invalid request");
			}

			mentor.AccessSessionId = sessionId;

			_ = await _unitOfWork.completeAsync();

			return Ok("Success");
		}
		[HttpGet]
		public async Task<IActionResult> GeneralStanding()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return BadRequest("Invalid request");
			}
			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;
			return Ok(await _headServices.GeneralStandingsAsync(campId));
		}
		[HttpGet]
		public async Task<IActionResult> MentorAttendence()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return BadRequest("Invalid request");
			}

			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			return Ok(await _headServices.MentorAttendence(campId));
		}
		[HttpGet]
		public async Task<IActionResult> TraineeAttendence()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return BadRequest("Invalid request");
			}

			var campId = _unitOfWork.HeadofCamp.GetByUserId(userId).Result?.CampId;

			
			return Ok(await _headServices.TraineeAttendence(campId));
		}
	}
}
