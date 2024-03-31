using AutoMapper;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.EF;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISC.Services.Services.ModelSerivces
{
    public class TraineeService : ITraineeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        public TraineeService
            (IUnitOfWork unitOfWork,
            UserManager<UserAccount> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }
        public async Task<ServiceResponse<object>> MentorInfoAsync(string traineeId)
        {
            var mentor = _unitOfWork.Trainees.Get()
                                            .Include(t => t.Mentor)
                                            .SingleOrDefaultAsync(t => t.UserId == traineeId)
                                            .Result?.Mentor;
            if (mentor == null)
            {
                throw new KeyNotFoundException("trainee not assigned to anyone");
            }

            var mentorAcc = await _userManager.FindByIdAsync(mentor.UserId);

            ServiceResponse<object> response = new ServiceResponse<object>
            {
                IsSuccess = true,
                Entity = new
                {
                    mentorAcc.Id,
                    FullName = mentorAcc.FirstName + ' ' + mentorAcc.MiddleName + ' ' + mentorAcc.LastName,
                    mentorAcc.PhotoUrl,
                    mentorAcc.Email,
                    mentorAcc.Grade,
                    mentorAcc.College,
                    mentor.About,
                    mentorAcc.FacebookLink,
                    mentorAcc.CodeForceHandle
                }
            };

            return response;
        }
        public async Task<string?> CampNameOfAsync(string traineeId)
        {
            return _unitOfWork.Trainees.Get()
                                .Include(t => t.Camp)
                                .SingleOrDefaultAsync(t => t.UserId == traineeId)
                                .Result?.Camp.Name;
        }
        public async Task<List<object>> AccessSheetsWithMaterialsAsync(string userId)
        {
            var traineeId = _unitOfWork.Trainees.GetByUserIdAsync(userId).Result.Id;

            var accessSheets = await _unitOfWork.TraineesSheetsAccess.FindManyWithChildAsync(tsa => tsa.TraineeId == traineeId);

            var sheetsIds = accessSheets.DistinctBy(a => a.SheetId).Select(i => i.SheetId).ToList();
            var Sheets = _unitOfWork.Sheets.Get()
                                    .Include(s => s.Materials)
                                    .Where(s => sheetsIds.Contains(s.Id))
                                    .OrderBy(s => s.SheetOrder)
                                    .ToHashSet();

            List<object> accessOn = new List<object>();
            foreach (var acc in accessSheets)
            {
                var sheet = Sheets.First(s => s.Id == acc.SheetId);

                accessOn.Add(new
                {
                    sheet.ProblemsCount,
                    sheet.Name,
                    sheet.Id,
                    sheet.SheetLink,
                    sheet.MinimumPrecent,
                    Solved = acc.NumberOfProblems,
                    Materials = sheet.Materials.Select(m => new
                    {
                        m.Name,
                        m.Link
                    }).ToList()
                });

                if (acc.NumberOfProblems / sheet.ProblemsCount * 100 < sheet.MinimumPrecent)
                    break;
            }

            return accessOn;
        }
        public async Task<object?> GetTasks(string traineeId)
        {
            return _unitOfWork.Trainees.Get()
                    .Include(s => s.Tasks)
                    .SingleOrDefaultAsync(t => t.UserId == traineeId)
                    .Result?.Tasks.Select(t => new { t.Task, t.IsComplete }).ToList();
        }
        public async Task UpdateTaskState(int taskId)
        {
            var task = await _unitOfWork.Tasks.FindByAsync(t => t.Id == taskId);
            task.IsComplete = !task.IsComplete;

            await _unitOfWork.Tasks.UpdateAsync(task);
        }
        public async Task<ServiceResponse<bool>> AddFeedback(SessionFeedbackDto model, string userId)
        {
            var response = new ServiceResponse<bool>() { IsSuccess = true };
            int traineeId = _unitOfWork.Trainees.FindByAsync(t => t.UserId == userId).Result.Id;

            var feedback = _mapper.Map<SessionFeedback>(model);
            feedback.TraineeId = traineeId;

            await _unitOfWork.SessionsFeedbacks.AddAsync(feedback);

            return response;
        }
        public async Task<ServiceResponse<TraineeInfoDto>> DisplayTraineeInfoAsync(string userId)
        {
            var response = new ServiceResponse<TraineeInfoDto>();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.Comment = "user not found";
                return response;
            }
            var traineeInfo = await _unitOfWork.Trainees.Get().SingleOrDefaultAsync(x => x.UserId == userId);

            if (traineeInfo is null)
            {
                response.Comment = "user not found";
                return response;
            }

            response.Entity = _mapper.Map<TraineeInfoDto>(user);
            response.Entity.points = traineeInfo.points;

            var traineesSolvedProblems = await _unitOfWork.TraineesSheetsAccess.Get()
                .GroupBy(x => x.TraineeId).Select(x => new
                {
                    TraineeId = x.Key,
                    SolvedProblems = x.Count()
                }).OrderByDescending(x => x.SolvedProblems).ToListAsync();

            for (int i = 0; i < traineesSolvedProblems.Count; i++)
            {
                if (traineesSolvedProblems[i].TraineeId == traineeInfo.Id)
                {
                    response.Entity.Rank = i + 1;
                    break;
                }
            }

            return response;
        }
    }
}
