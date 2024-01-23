using AutoMapper;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.EF;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using ISC.Services.Services.ExceptionSerivces.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace ISC.Services.Services.ModelSerivces
{
	public class HeadServices:IHeadSerivces
    {
		private readonly UserManager<UserAccount> _userManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOnlineJudgeServices _onlineJudgeServices;
		private readonly CodeForceConnection _codeForceConnection;
		private readonly IMapper _mapper;

        public HeadServices(UserManager<UserAccount> userManager,
            IUnitOfWork unitofwork,
            IOnlineJudgeServices onlineJudgeServices,
            IOptions<CodeForceConnection> connection,
            IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitofwork;
            _onlineJudgeServices = onlineJudgeServices;
            _codeForceConnection = connection.Value;
            _mapper = mapper;
        }
        public async Task<ServiceResponse<object>> DisplayTrainees(string userId)
		{
			var response = new ServiceResponse<object>() { IsSuccess = true };

			var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

			if(campId is null)
			{
				throw new BadRequestException("Head Camp info error");
			}

			response.Entity = await _userManager.Users.Include(u=>u.Trainee).Where(u=>u.Trainee!= null &&u.Trainee.CampId==campId)
				.Select(tr => new
				{
					tr.Id,
					FullName = tr.FirstName + ' ' + tr.MiddleName + ' ' + tr.LastName,
					tr.Email,
					tr.CodeForceHandle,
					tr.Gender,
					tr.Grade
				}).ToListAsync();

			return response;
        }
		public async Task<ServiceResponse<List<TraineeMentorDto>>> DisplayTraineeMentorAsync(string userId)
		{
			var response = new ServiceResponse<List<TraineeMentorDto>>();

			var campId = _unitOfWork.HeadofCamp.findWithChildAsync(h => h.UserId == userId, new[] { "Camp" }).Result?.CampId;

			var trainees =await _userManager.Users
				.Include(u => u.Trainee)
				.Where(u => u.Trainee != null && u.Trainee.CampId == campId)
				.ToListAsync();

			var mentors = await _userManager.Users
				.Include(u => u.Mentor)
				.Where(u => u.Mentor != null)
				.ToListAsync();

			var traineeMentor = trainees.Join(mentors, t => t.Trainee.MentorId, m => m.Mentor.Id,
				(t, m) => new TraineeMentorDto()
				{
					TraineeId = t.Trainee.Id,
					MentorId = m.Mentor?.Id,
					TraineeName = t.FirstName + ' ' + t.MiddleName + " " + t.LastName,
					MentorName = m.Mentor != null ? m.FirstName + ' ' + m.MiddleName + " " + m.LastName : null
				}).ToList();

			response.IsSuccess = true;
			response.Entity = traineeMentor;

			if (traineeMentor.IsNullOrEmpty())
			{
				response.Entity = new List<TraineeMentorDto>();
			}

			return response;
		}
		public async Task SubmitTraineeMentorAsync(List<AssignTraineeMentorDto> data)
		{
			foreach(var item in data)
			{
				var trainee = await _unitOfWork.Trainees.getByIdAsync(item.TraineeId);
				if(trainee is null)
				{
					continue;
				}
				var mentor = await _unitOfWork.Mentors.getByIdAsync(item.MentorId);
				trainee.Mentor= mentor;
				trainee.MentorId = item.MentorId;
				await _unitOfWork.Trainees.UpdateAsync(trainee);
			}
			await _unitOfWork.completeAsync();
		}
		public async Task<ServiceResponse<TraineeSheetAcessDto>> DisplayTraineeAccess(int campId)
		{
			var response=new ServiceResponse<TraineeSheetAcessDto>() { IsSuccess = true };

			var trainees = _userManager.Users
						.Include(u => u.Trainee)
						.Where(u => u.Trainee != null && u.Trainee.CampId == campId)
						.Select(u => new
						{
							traineeId = u.Trainee.Id,
							FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName
						});

			var sheets= await _unitOfWork.Sheets.Get().
						Where(s=>s.CampId==campId)
						.Select(s => new
						{
							s.Id,
							s.Name
						}).ToListAsync();

			List<TraineeSheetAccess> dbSheetAccess = new List<TraineeSheetAccess>();
			response.Entity = new TraineeSheetAcessDto();

			if (sheets is null)
			{
				return response;
			}

			dbSheetAccess = await _unitOfWork.TraineesSheetsAccess
						.Get()
						.Where(ac => sheets.Any(s => s.Id == ac.SheetId))
						.ToListAsync();

			response.Entity.Sheets = sheets.Select(s => new SheetsInfo()
			{
				Id = s.Id,
				Name = s.Name,
			}).ToList();

			//loop on trainee / find trainee with acces / add to status 
			foreach(var trainee in trainees)
			{
				TraineeAccess access=new TraineeAccess();
				access.FullName = trainee.FullName;

				foreach(var sheet in sheets)
				{
					access.Statues.Add(new Access()
					{
						SheetId = sheet.Id,
						HasAccess = dbSheetAccess.Any(da => da.TraineeId == trainee.traineeId && da.SheetId == sheet.Id)
					});
				}
				response.Entity.TraineeAccess.Add(access);
			}

			return response;
		}
		public async Task AddNewTrainingSheetAccess(int sheetId,int campId)
		{
			if (campId == 0)
			{
				throw new KeyNotFoundException("Unvalid campid");
			}

			var accessing = await _unitOfWork.Trainees.Get()
							.Where(t => t.CampId == campId)
							.Select(t => new TraineeSheetAccess()
							{
								TraineeId = t.Id,
								SheetId = sheetId,
							}).ToListAsync();
			await _unitOfWork.TraineesSheetsAccess.AddGroup(accessing);
		}
		public async Task<List<TraineeStandingDto>> GeneralStandingsAsync(int? campId)
		{

			if (campId is null)
			{
				return new List<TraineeStandingDto>();
			}

			var response = new List<TraineeStandingDto>();

			var sheets = await _unitOfWork.Sheets.Get()
							.Where(s => s.CampId == campId)
							.Select(s => new {
								s.Id,
								s.Name,
								Total = _onlineJudgeServices.GetContestStandingAsync(
								s.SheetCfId,
								1,
								true,
								s.IsSohag ? _codeForceConnection.SohagKey : _codeForceConnection.AssuitKey,
								s.IsSohag ? _codeForceConnection.SohagSecret : _codeForceConnection.AssuitSecret).Result
							}).ToListAsync();

			var Trainees = await _userManager.Users
						.Include(u => u.Trainee)
						.Where(u => u.Trainee != null && u.Trainee.CampId == campId)
						.Select(u => new { u.Trainee.Id, FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName })
			.ToListAsync();

			var traineeSheets = await _unitOfWork.TraineesSheetsAccess.Get()
								.Where(acc =>
									sheets.Select(i => i.Id).ToList()
									.Contains(acc.SheetId))
								.ToListAsync();

			foreach (var trainee in Trainees)
			{
				var traineeStanding = new TraineeStandingDto() { Name = trainee.FullName };

				foreach (var sheet in sheets)
				{
					traineeStanding.stand.Add(new SheetInfo()
					{
						Id = sheet.Id,
						Name = sheet.Name,
						Total = sheet.Total == null ? 26 : sheet.Total.result.problems.Count,
						Count = traineeSheets.FirstOrDefault(s => s.SheetId == sheet.Id && s.TraineeId == trainee.Id)?
										.NumberOfProblems ?? 0
					});
				}
				response.Add(traineeStanding);
			}

			return response;
		}
		public async Task<PersonAttendenceDto> TraineeAttendence(int? campId)
		{
			if(campId is null)
			{
				throw new KeyNotFoundException("Invalid reqeust");
			}

			var trainees = await _userManager.Users.Include(u => u.Trainee).Include(u => u.Trainee.TraineesAttendences)
						.Where(u => u.Trainee != null).Select(u => new
						{
							u.Trainee.Id,
							FullName=u.FirstName+' '+u.MiddleName+' '+u.LastName,
							AttendenceHistory=u.Trainee.TraineesAttendences.ToList(),

						}).ToListAsync();
			var sessions = _unitOfWork.Sessions.findManyWithChildAsync(s => s.CampId == campId).Result.Select(s => new
			{
				s.Id,
				s.Topic
			});

			var response = new PersonAttendenceDto()
			{
				Sessions = sessions.Select(s => s.Topic).ToList()
			};

			foreach (var trainee in trainees)
			{
				var info = new PersonInfoDto() { FullName = trainee.FullName };

				foreach(var session in sessions)
				{
					info.IsAttend.Add(trainee.AttendenceHistory.Any(h => h.TraineeId == trainee.Id && h.SessionId == session.Id));
				}
				response.Attendances.Add(info);
			}

			return response;
		}
		public async Task<PersonAttendenceDto>MentorAttendence(int? campId)
		{
			var response = new PersonAttendenceDto();
			if(campId is null)
			{
				throw new ArgumentNullException("camp id can not be null");
			}
			var mentorsId = _unitOfWork.Mentors.findManyWithChildAsync(i => i.Camps.Any(c => c.Id == campId), new[] { "Camps" })
							.Result.Select(m=>m.Id).ToList();

			if (mentorsId.IsNullOrEmpty())
			{
				throw new BadHttpRequestException("no mentors for this camp");
			}
			var mentors = await _userManager.Users
						.Include(u => u.Mentor)
						.Where(u => u.Mentor != null && mentorsId.Contains(u.Mentor.Id))
						.Select(ac => new
						{
							ac.Mentor.Id,
							FullName=ac.FirstName+' '+ac.MiddleName+' '+ac.LastName
						})
						.ToListAsync();

			var sessions = await _unitOfWork.Sessions.findManyWithChildAsync(s => s.Date <= DateTime.UtcNow);
			response.Sessions = sessions.Select(i => i.Topic).ToList();

			var attendence = await _unitOfWork.MentorAttendence.findManyWithChildAsync(a => mentorsId.Contains(a.MentorId)
							&& sessions.Select(s => s.Id).Contains(a.SessionId));

			foreach(var mentor in mentors)
			{
				var mentorInfo = new PersonInfoDto();
				foreach(var session in sessions)
				{
					mentorInfo.IsAttend.Add(attendence.Any(a => a.MentorId == mentor.Id && a.SessionId == session.Id));
				}
				response.Attendances.Add(mentorInfo);
			}

			return response;
		}
		public async Task<ServiceResponse<bool>> DeleteFromTrianee(List<string> traineesUsersId)
		{
			var response= new ServiceResponse<bool>() { IsSuccess = true };

            foreach (string traineeuserid in traineesUsersId)
            {
                var traineeAccount = await _userManager.Users
					.Include(i => i.Trainee)
					.Where(user => user.Id == traineeuserid)
					.SingleOrDefaultAsync();

                if (traineeAccount != null)
                {
                    var camp = await _unitOfWork.Trainees.getCampofTrainee(traineeAccount.Trainee.Id);
					var archive = _mapper.Map<TraineeArchive>(traineeAccount);
					archive.CampName = camp.Name;
                    /*TraineeArchive Archive = new TraineeArchive()
                    {
                        FirstName = traineeAccount.FirstName,
                        MiddleName = traineeAccount.MiddleName,
                        LastName = traineeAccount.LastName,
                        NationalID = traineeAccount.NationalId,
                        BirthDate = traineeAccount.BirthDate,
                        Grade = traineeAccount.Grade,
                        Gender = traineeAccount.Gender,
                        College = traineeAccount.College,
                        CodeForceHandle = traineeAccount.CodeForceHandle,
                        FacebookLink = traineeAccount.FacebookLink,
                        VjudgeHandle = traineeAccount.VjudgeHandle,
                        Email = traineeAccount.Email,
                        PhoneNumber = traineeAccount.PhoneNumber,
                        CampName = camp.Name,
                        IsCompleted = false
                    };*/
                    await _unitOfWork.TraineesArchive.addAsync(archive);
                    await _userManager.DeleteAsync(traineeAccount);
                }
            }
            await _unitOfWork.completeAsync();

			return response;
        }
	}
}
