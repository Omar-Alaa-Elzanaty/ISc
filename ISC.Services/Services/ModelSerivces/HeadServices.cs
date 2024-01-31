using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using ISC.Core.APIDtos;
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
		private readonly ISheetServices _sheetServices;
		private readonly ISessionsServices _sessionsServices;
		private readonly IMailServices _mailServices;
		private readonly IMapper _mapper;

        public HeadServices(UserManager<UserAccount> userManager,
            IUnitOfWork unitofwork,
            IOnlineJudgeServices onlineJudgeServices,
            IOptions<CodeForceConnection> connection,
            IMapper mapper,
            ISheetServices sheetServices,
            ISessionsServices sessionsServices,
            IMailServices mailServices)
        {
            _userManager = userManager;
            _unitOfWork = unitofwork;
            _onlineJudgeServices = onlineJudgeServices;
            _codeForceConnection = connection.Value;
            _mapper = mapper;
            _sheetServices = sheetServices;
            _sessionsServices = sessionsServices;
            _mailServices = mailServices;
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
		public async Task<ServiceResponse<bool>> SubmitTraineeMentorAsync(List<AssignTraineeMentorDto> data)
		{
			var response = new ServiceResponse<bool>() { IsSuccess = true };

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

			return response;
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
                    var camp = await _unitOfWork.Trainees.GetCampOfTrainee(traineeAccount.Trainee.Id);
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
		public async Task<ServiceResponse<List<KeyValuePair<FilteredUserDto, string>>>> WeeklyFilterAsync
			(List<string>selectedTrainees,
			string headId)
		{
			var response = new ServiceResponse<List<KeyValuePair<FilteredUserDto, string>>>();

            var camp = _unitOfWork.HeadofCamp.findWithChildAsync(t => t.UserId == headId,
                                                                new[] { "Camp", })?.Result?.Camp ?? null;
            var traineesId = await _unitOfWork.Trainees
                            .Get()
                            .Where(t => selectedTrainees.Contains(t.UserId)).Select(t => t.Id)
                            .ToListAsync();
			if(camp is null || traineesId.IsNullOrEmpty())
			{
				throw new KeyNotFoundException("Maybe not found camp or trainees for selected camp");
			}

            var result = await _sheetServices.TraineeSheetAccesWithout(traineesId, camp?.Id ?? 0);

            if (!result.IsSuccess)
            {
				throw new KeyNotFoundException("No trainees found");
            }

			List<TraineeSheetAccess> traineesAccess = result.Entity!;

			var problemsSheetCount = _sheetServices.TraineeSheetProblemsCount(traineesAccess).Result.Entity!;
            var filteredOnSheets = _sheetServices.TraineesFilter(traineesAccess, problemsSheetCount).Result.Entity;

            var traineesIds = traineesAccess.Select(i => i.TraineeId).ToList();
            var filteredOnSessions = _sessionsServices.SessionFilter(traineesIds).Result.Entity;

            List<KeyValuePair<FilteredUserDto, string>> filtered = new List<KeyValuePair<FilteredUserDto, string>>();

            for (int trainee = 0; trainee < traineesAccess.Count(); trainee++)
            {
                bool foundInSheetFilter = filteredOnSheets?.Contains(traineesAccess[trainee].TraineeId) ?? false;
                bool foundInSessionFilter = filteredOnSessions?.Contains(traineesAccess[trainee].TraineeId) ?? false;

                if (foundInSheetFilter || foundInSessionFilter)
                {
                    var TraineeAccount = await _userManager.FindByIdAsync(traineesAccess[trainee].Trainee.UserId);
                    if (TraineeAccount != null)
                    {
                        var FilteredUser = new FilteredUserDto()
                        {
                            UserId = TraineeAccount.Id,
                            FirstName = TraineeAccount.FirstName,
                            MiddleName = TraineeAccount.MiddleName,
                            LastName = TraineeAccount.LastName,
                            Email = TraineeAccount.Email!,
                            PhoneNumber = TraineeAccount.PhoneNumber!,
                            CodeforceHandle = TraineeAccount.CodeForceHandle,
                            College = TraineeAccount.College,
                            Gender = TraineeAccount.Gender,
                            Grade = TraineeAccount.Grade
                        };

                        StringBuilder Reason = new StringBuilder();

                        if (foundInSheetFilter)
                        {
                            Reason.Append("Sheets");
                        }

                        if (foundInSessionFilter)
                        {
                            Reason.Append(Reason.Length != 0 ? "/Sessions" : "Sessions");
                        }

						filtered.Add(new(FilteredUser, Reason.ToString()));
                    }
                }
            }

			response.IsSuccess = true;
			response.Entity=filtered;

			return response;
        }
		public async Task<ServiceResponse<object>> SubmitWeeklyFilterAsync(List<string>traineesUsersId,string headUserId)
		{
			var response = new ServiceResponse<object>();

            var camp = _unitOfWork.HeadofCamp.findWithChildAsync(t => t.UserId == headUserId, new[] { "Camp", }).Result?.Camp;

			if(camp is null)
			{
				throw new KeyNotFoundException("Couldn't found Camp");
			}

            List<UserAccount> Fail = new List<UserAccount>();

            foreach (var id in traineesUsersId)
            {
                var traineeAccount = await _userManager.FindByIdAsync(id);

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
                        Email = traineeAccount.Email!,
                        PhoneNumber = traineeAccount.PhoneNumber,
                        Year = camp.Year,
                        CampName = camp.Name,
                        IsCompleted = false
                    };

                    var Result = await _mailServices.sendEmailAsync(traineeAccount.Email!, "ICPC Sohag Filteration announcement"
                        , $"Hello {traineeAccount.FirstName} + ' ' + {traineeAccount.MiddleName},{@"<\br>"} We regret to inform you that we had to remove you from the {camp.Name} training program." +
                        $" If you're interested in exploring other training programs, please let us know, and we'll provide you with more information." +
                        $" Thank you for your efforts, and we hope you'll take this as a learning experience to continue your growth and development." +
                        $"{@"<\br>"}{@"<\br>"}Best regards,{@"<\br>"}{@"<\br>"} ISc System{@"<\br>"}{@"<\br>"} Omar Alaa");

                    if (Result)
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

			response.IsSuccess = true;
			response.Entity = new { Fail, Comment = Fail.IsNullOrEmpty() ? "" : "Send email failure" };


            return response;
        }
		public async Task<ServiceResponse<List<object>>> DisplayMentorsAsync(string userId)
		{
			var response= new ServiceResponse<List<object>>() { IsSuccess = true };

            var camp = _unitOfWork.HeadofCamp
                .Get()
                .Include(h => h.Camp)
                .FirstOrDefaultAsync(h => h.UserId == userId).Result?.Camp;

            List<object> mentors = new List<object>();

            var mentorsOfCamp = await _unitOfWork.Camps.findWithChildAsync(c => c.Id == camp!.Id, new[] { "Mentors" });

            foreach (var member in mentorsOfCamp!.Mentors)
            {
                var userInfo = await _userManager.FindByIdAsync(member.UserId);
                mentors.Add(new
                {
                    member.Id,
                    member.UserId,
                    FullName = userInfo!.FirstName + ' ' + userInfo.MiddleName + " " + userInfo.LastName,
                });
            }

			response.Entity = mentors;

			return response;
        }
		public async Task<ServiceResponse<List<Session>>>DisplaySessionsAsync(string userId)
		{
			var response=new ServiceResponse<List<Session>>() { IsSuccess = true };

            var headOfCamp = await _unitOfWork.HeadofCamp.GetByUserIdAsync(userId);

            if (headOfCamp is null)
            {
                throw new BadRequestException("Error in account");
            }

            response.Entity= await _unitOfWork.Sessions.findManyWithChildAsync(s => s.CampId == headOfCamp.CampId);

			return response;
        }
		public async Task<ServiceResponse<int>>AddSessionAsync(SessionDto model,string userId)
		{
			var response = new ServiceResponse<int> { IsSuccess = true };

            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

            if (campId is null)
            {
                throw new BadRequestException("no camp found");
            }

            model.Topic = model.Topic.ToLower();
            model.InstructorName = model.InstructorName.ToLower();

			var isFound = await _unitOfWork.Sessions
				.Get()
				.AnyAsync(s => !((s.Topic == model.Topic && s.CampId == campId) ||
									(s.Date.Day == model.Date.Day && s.Date.Month == model.Date.Month && campId == model.CampId)));

            if (!isFound)
            {
                throw new BadRequestException("this session may record before or Conflict with other session");
            }

            Session session = _mapper.Map<Session>(model);
            session.CampId = (int)campId;

            await _unitOfWork.Sessions.addAsync(session);
            _ = await _unitOfWork.completeAsync();

			response.Entity = session.Id;

			return response;
        }
		public async Task<ServiceResponse<int>>DeleteSessionAsync(int id)
		{
			var response = new ServiceResponse<int>() { IsSuccess = true };
            var session = await _unitOfWork.Sessions.getByIdAsync(id);

            if (session is null)
            {
                throw new BadRequestException("no session found");
            }

            _ = await _unitOfWork.Sessions.deleteAsync(session);
            _ = await _unitOfWork.completeAsync();

            return response;
        }
		public async Task<ServiceResponse<int>>UpdateSessionInfoAsync(SessionDto session,int id)
		{
			var response = new ServiceResponse<int>() { IsSuccess = true };

            var entity = await _unitOfWork.Sessions.getByIdAsync(id);

            if (entity is null)
            {
                throw new KeyNotFoundException("Invalid session");
            }

            var validResponse = await _unitOfWork.Sessions.CheckUpdateAbility(entity, session, id);

            if (!validResponse.IsSuccess)
            {
                throw new BadRequestException(validResponse.Comment);
            }

			entity = _mapper.Map<Session>(entity);

            await _unitOfWork.Sessions.UpdateAsync(entity);
            _ = await _unitOfWork.completeAsync();

			response.Entity = entity.Id;
			return response;
        }
    }
}
