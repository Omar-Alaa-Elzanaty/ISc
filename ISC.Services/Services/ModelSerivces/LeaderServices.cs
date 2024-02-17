using AutoMapper;
using ISC.Core.APIDtos;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using ISC.Services.Services.ExceptionSerivces.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace ISC.Services.Services.ModelSerivces
{
    public class LeaderServices : ILeaderServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly IAuthanticationServices _authServices;
        private readonly IMediaServices _mediaServices;
        private readonly DataBase _context;
        private readonly ISheetServices _sheetServices;

        public LeaderServices(IUnitOfWork unitOfWork,
            UserManager<UserAccount> userManager,
            IAuthanticationServices authanticationServices,
            IMapper mapper,
            IMediaServices mediaServices,
            DataBase context,
            ISheetServices sheetServices)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;

            _authServices = authanticationServices;
            _mapper = mapper;
            _mediaServices = mediaServices;
            _context = context;
            _sheetServices = sheetServices;
        }
        public async Task<ServiceResponse<bool>> DeleteTraineesAsync(List<DeleteTraineeDto> trainees)
        {
            ServiceResponse<bool> response = new ServiceResponse<bool>() { IsSuccess = true };

            var users = await _userManager.Users
                                    .Include(a => a.Trainee)
                                    .Include(a => a.Trainee!.Camp)
                                    .Where(a => a.Trainee != null && trainees.Select(t => t.TraineeId).ToList().Contains(a.Id))
                                    .ToListAsync();

            List<TraineeArchive> archive = new List<TraineeArchive>();

            foreach (var user in users)
            {
                var trainee = _mapper.Map<TraineeArchive>(user);
                trainee.CampName = user.Trainee!.Camp.Name;
                trainee.IsCompleted = trainees.First(t => t.TraineeId == user.Id).IsComplete;

                archive.Add(trainee);

                await _mediaServices.DeleteAsync(user.PhotoUrl);
                await _userManager.DeleteAsync(user);
            }

            await _unitOfWork.TraineesArchive.AddGroup(archive);
            await _unitOfWork.completeAsync();

            return response;
        }
        public async Task<ServiceResponse<Camp>> AddCampAsync(CampDto camp)
        {
            ServiceResponse<Camp> response = new ServiceResponse<Camp>() { IsSuccess = true };

            var campItem = await _unitOfWork.Camps.findByAsync(c => c.Name == camp.Name);

            if (campItem is not null)
            {
                throw new BadRequestException("Camp is already exist");
            }

            Camp newCamp = _mapper.Map<Camp>(camp);

            await _unitOfWork.Camps.addAsync(newCamp);
            int result = await _unitOfWork.completeAsync();

            if (result == 0)
            {
                throw new BadRequestException("Couldn't add camp");
            }

            response.IsSuccess = true;
            response.Entity = newCamp;
            return response;
        }
        public async Task<ServiceResponse<List<string>>> AddToRoleAsync(UserRoleDto model)
        {
            ServiceResponse<List<string>> response = new ServiceResponse<List<string>>();
            List<string> faillToAdd = new List<string>();

            foreach (var userId in model.Users)
            {
                var user = await _userManager.FindByIdAsync(userId);
                bool isSuccess = await _unitOfWork.addToRoleAsync<UserAccount>(user, model.Role, model.CampId, model.MentorId);
                if (!isSuccess)
                    faillToAdd.Add(user.FirstName + ' ' + user.MiddleName + ' ' + user.LastName);
            }

            if (faillToAdd.Count > 0)
            {
                response.Entity = faillToAdd;
                response.Comment = $"some users couldn't add to role {model.Role}";
                return response;
            }

            response.IsSuccess = true;
            return response;
        }
        public async Task<ServiceResponse<object>> DisplayNewRegisterAsync(int campId)
        {
            var response = new ServiceResponse<object>() { IsSuccess = true };

            var applications = await _unitOfWork.NewRegitseration.Get()
                                .Where(app => app.CampId == campId)
                                .Select(app => new
                                {
                                    app.FirstName,
                                    app.MiddleName,
                                    app.LastName,
                                    app.Gender,
                                    app.College,
                                    app.CodeForceHandle,
                                    app.VjudgeHandle,
                                    app.Grade,
                                    app.NationalID,
                                    app.PhoneNumber,
                                    app.HasLaptop,
                                    app.Comment,
                                }).ToListAsync();

            response.Entity = applications;

            return response;
        }
        public async Task<ServiceResponse<bool>> DeleteFromNewRegister(List<string> ids)
        {
            var response = new ServiceResponse<bool>();

            var registers = await _unitOfWork.NewRegitseration
                .findManyWithChildAsync(r => ids.Contains(r.NationalID));

            if (registers.IsNullOrEmpty())
            {
                response.Comment = "No data found";
                return response;
            }

            response.IsSuccess = true;

            _unitOfWork.NewRegitseration.deleteGroup(registers);
            _ = await _unitOfWork.completeAsync();

            return response;
        }
        public async Task<ServiceResponse<bool>> AssignRoleToStuff(StuffNewRolesDto model)
        {
            var response = new ServiceResponse<bool>() { IsSuccess = true };



            var userRole = model.UserRole;

            var accounts = new List<UserAccount>();
            foreach (var userid in model.UsersIds)
            {
                var Account = await _userManager.FindByIdAsync(userid);
                if (Account == null)
                {
                    throw new KeyNotFoundException("One of accounts not exist!");
                }
                accounts.Add(Account);
            }
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var account in accounts)
                    {
                        bool Result = await _unitOfWork.addToRoleAsync(account, userRole.Role, userRole.CampId, null);

                        if (Result == false)
                        {
                            throw new ServerErrorExeption($"Can't save user to these role {userRole.Role}");
                        }
                    }

                    await trans.CommitAsync();
                    await _unitOfWork.completeAsync();
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();

                    throw;
                }
            }

            response.IsSuccess = true;
            return response;
        }
        public async Task<ServiceResponse<List<string>>> DeleteStuffAsync(List<string> stuffsIds)
        {
            var response = new ServiceResponse<List<string>>() { Entity = new List<string>(), IsSuccess = true };

            foreach (string userId in stuffsIds)
            {
                var account = await _userManager.FindByIdAsync(userId);

                if (account == null)
                {
                    throw new BadRequestException("Some accounts not exist");
                }

                var userRoles = _userManager.GetRolesAsync(account).Result.ToList();
                bool result = true;

                if (userRoles.Contains(Role.MENTOR))
                {
                    result = await _unitOfWork.Mentors.deleteAsync(userId);
                }
                if (userRoles.Contains(Role.HOC) && result == true)
                {
                    result = await _unitOfWork.HeadofCamp.deleteEntityAsync(userId);
                }
                if (result == true)
                {
                    StuffArchive archive = _mapper.Map<StuffArchive>(account);

                    await _unitOfWork.StuffArchive.addAsync(archive);
                    await _userManager.DeleteAsync(account);

                    await _mediaServices.DeleteAsync(account.PhotoUrl);
                }
                else
                {
                    response.Entity.Add(account.FirstName + ' ' + account.MiddleName);
                }

                await _unitOfWork.completeAsync();
            }

            if (response.Entity.Count() > 0)
            {
                response.IsSuccess = false;
                response.Comment = "Some members couldn't delete,if you try to delete mentor please check that this mentor doesn't assign to any trainee";
            }

            return response;
        }
        public async Task<ServiceResponse<string>> DeleteTraineeArchivesAsync(List<string> trainees)
        {
            var response = new ServiceResponse<string>();

            if (trainees == null || trainees.Count() == 0)
            {
                throw new BadRequestException("Invalid request");
            }

            var archives = await _unitOfWork.TraineesArchive.findManyWithChildAsync(ta => trainees.Contains(ta.NationalID));

            if (trainees == null || trainees.Count == 0)
            {
                throw new BadRequestException("No account to remove");
            }

            _unitOfWork.TraineesArchive.deleteGroup(archives);
            _ = await _unitOfWork.completeAsync();

            response.IsSuccess = true;
            response.Entity = "Deleted Successfully";

            return response;
        }
        public async Task<ServiceResponse<bool>> UpdateTraineeArchive(HashSet<TraineeArchiveDto> archives)
        {
            var response = new ServiceResponse<bool>() { IsSuccess = true };

            var nationalIds = archives.Select(a => a.NationalId).ToHashSet();

            var memebers = await _unitOfWork.TraineesArchive.Get()
                        .Where(i => nationalIds.Contains(i.NationalID))
                        .ToListAsync();

            foreach (var archive in archives)
            {
                var trainee = memebers.Single(m => m.NationalID == archive.NationalId);
                trainee = _mapper.Map<TraineeArchive>(archive);
            }
            _ = await _unitOfWork.completeAsync();

            return response;
        }
        public async Task<ServiceResponse<bool>> UpdateStuffArchive(HashSet<StuffArchiveDto> archives)
        {
            var response = new ServiceResponse<bool>() { IsSuccess = true };

            var nationalIds = archives.Select(a => a.NationalID).ToHashSet();

            var members = _unitOfWork.StuffArchive.Get()
                        .Where(s => nationalIds.Contains(s.NationalID)).ToHashSet();

            foreach (var stuffMember in archives)
            {
                var stuff = members.Single(m => m.NationalID == stuffMember.NationalID);
                stuff = _mapper.Map<StuffArchive>(stuff);
            }

            await _unitOfWork.completeAsync();

            response.IsSuccess = true;
            response.Comment = "Update successfully";

            return response;
        }
        public async Task<ServiceResponse<AuthModel>> SubmitNewRegister(SubmitNewRegisterDto newRegisters)
        {
            var response = new ServiceResponse<AuthModel>();

            HashSet<string> refused = new HashSet<string>();

            foreach (var contest in newRegisters.ContestsInfo)
            {
                var standingResponse = await _sheetServices.SheetStanding(contest.ContestId, contest.IsSohag);
                if (!standingResponse.IsSuccess)
                {
                    throw new BadRequestException(standingResponse.Comment);
                }

                var statusResponse = await _sheetServices.SheetStatus(contest.ContestId, contest.IsSohag);
                if (!statusResponse.IsSuccess)
                {
                    throw new BadRequestException(statusResponse.Comment);
                }


                var memberPerProblem = statusResponse.Entity?.GroupBy(submission =>
                new
                {
                    Handle = submission.author.members.FirstOrDefault()?.handle,
                    ProblemName = submission.problem.name
                }).Where(mps => mps.Any(sub => sub.verdict == "OK")).Select(mps => new
                {
                    mps.Key.ProblemName,
                    mps.Key.Handle
                }).GroupBy(mps => mps.Handle).Select(problemSolved => new
                {
                    handle = problemSolved.Key,
                    Count = problemSolved.Count()
                }).ToList();

                float totalproblems = standingResponse.Entity.problems.Count();

                foreach (var member in memberPerProblem)
                {
                    if (Math.Ceiling(member.Count / totalproblems) * 100.0 < contest.PassingPrecent)
                    {
                        refused.Add(member.handle);
                    }
                }
            }

            var PassedMember = await _unitOfWork.NewRegitseration
                .findManyWithChildAsync(nr => !refused.Contains(nr.CodeForceHandle)
                                        && newRegisters.CandidatesNationalId.Contains(nr.NationalID) == true);

            var camp = _unitOfWork.Camps.getByIdAsync(newRegisters.CampId).Result.Name;

            List<NewRegistration> faillRegisteration = new List<NewRegistration>();
            List<NewRegistration> confirmedAcceptacne = new List<NewRegistration>();

            foreach (var member in PassedMember)
            {
                var newTrainee = _mapper.Map<RegisterDto>(member);
                newTrainee.Role = Role.TRAINEE;
                newTrainee.CampId = newRegisters.CampId;

                response = await AutoMemberAddAsync(newTrainee, camp);

                if (!response.IsSuccess)
                {
                    faillRegisteration.Add(member);
                }
                else
                {
                    PassedMember.Add(member);
                }
            }

            _unitOfWork.NewRegitseration.deleteGroup(PassedMember);


            return response;
        }
        public async Task<ServiceResponse<int>> UpdateCampStatusAsync(int campId)
        {
            var response = new ServiceResponse<int>();

            var camp = await _unitOfWork.Camps.getByIdAsync(campId);

            if (camp == null)
            {
                throw new BadRequestException("Invalid Id");
            }

            camp.OpenForRegister = !camp.OpenForRegister;

            _ = await _unitOfWork.completeAsync();

            response.IsSuccess = true;
            response.Comment = $"Status changed to {camp.OpenForRegister}";

            return response;
        }
        public async Task<ServiceResponse<object>> CampInfo()
        {
            await Task.CompletedTask;
            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                Entity = await _unitOfWork.Camps.Get().Select(c => new
                {
                    c.Id,
                    c.Name
                }).ToListAsync()
            };
        }
        public async Task<ServiceResponse<object>> MentorInfo(int campId)
        {
            await Task.CompletedTask;

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                Entity = await _userManager.Users
                .Include(u => u.Mentor)
                .Include(u => u.Mentor!.Camps)
                .Where(u => u.Mentor != null && u.Mentor.Camps.Any(c => c.Id == campId))
                .Select(u => new
                {
                    u.Mentor!.Id,
                    FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName
                })
                .ToListAsync()
            };
        }
        private async Task<ServiceResponse<AuthModel>> AutoMemberAddAsync(RegisterDto registerInfo, string? message = null, string? campName = null)
        {
            ServiceResponse<AuthModel> response = new ServiceResponse<AuthModel>();
            AuthModel result = await _authServices.RegisterAsync(
                user: registerInfo,
                message: message,
                sendEmail: true
                );
            if (!result.IsAuthenticated)
            {
                response.IsSuccess = false;
                response.Comment = "Couldn't create account";

                return response;
            }

            response.IsSuccess = true;

            response.Entity = result;

            return response;
        }
    }
}
