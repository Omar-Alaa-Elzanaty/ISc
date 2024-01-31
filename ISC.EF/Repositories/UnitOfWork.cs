using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISC.EF.Repositories
{
    public class UnitOfWork : IUnitOfWork
	{
		private readonly UserManager<UserAccount> _userManager;
		public readonly DataBase _dataBase;
		private readonly IWebHostEnvironment _Host;
		private readonly IHttpContextAccessor _HttpContext;
		public ITraineeRepository Trainees { get; private set; }

		public ISessionRepository Sessions { get; private set; }

		public IMentorRepository Mentors { get; private set; }

		public ITraineeAttendenceRepository TraineesAttendence { get; private set; }
		public IBaseRepository<MentorAttendence> MentorAttendence { get;private set; }

		public ISheetRepository Sheets { get; private set; }

		public IBaseRepository<TraineeSheetAccess> TraineesSheetsAccess { get; private set; }

		public IHeadOfCampRepository HeadofCamp { get; private set; }

		public ICampRepository Camps { get; private set; }

		public ISessionFeedbackRepository SessionsFeedbacks { get; private set; }


		public IBaseRepository<Material> Materials { get; private set; }

		public IBaseRepository<TraineeArchive> TraineesArchive { get; private set; }

		public IBaseRepository<StuffArchive> StuffArchive { get; private set; }

		public IBaseRepository<NewRegistration> NewRegitseration { get; private set; }
		public IBaseRepository<TaskList>Tasks { get; private set; }
        public UnitOfWork(DataBase database,UserManager<UserAccount> usermanager,IWebHostEnvironment host, IHttpContextAccessor httpContext)
        {
			_dataBase = database;
			_userManager = usermanager;
			Trainees = new TraineeRepository(_dataBase,_userManager);
			Sessions= new SessionRepository(_dataBase);
			Mentors = new MentorRepository(database,usermanager);
			TraineesAttendence = new TraineeAttendenceRepository(_dataBase);
			MentorAttendence=new BaseRepository<MentorAttendence>(_dataBase);
			Sheets = new SheetRepository(_dataBase);
			TraineesSheetsAccess = new BaseRepository<TraineeSheetAccess>(_dataBase);
			HeadofCamp = new HeadOfCampRepository(_dataBase);
			Camps = new CampRepository(_dataBase);
			SessionsFeedbacks = new SessionFeedbackRepository(_dataBase);
			Materials = new BaseRepository<Material>(_dataBase);
			TraineesArchive = new BaseRepository<TraineeArchive>(_dataBase);
			StuffArchive = new BaseRepository<StuffArchive>(_dataBase);
			NewRegitseration = new BaseRepository<NewRegistration>(_dataBase);
			Tasks = new BaseRepository<TaskList>(_dataBase);
		}
		public async Task<bool> addToRoleAsync<T>(T account, string role,int?campId,int?mentorId)
		{
			if(account is UserAccount Acc)
			{
				if (Acc != null && _userManager.GetRolesAsync(Acc).Result.Contains(role) == true)
				{
					throw new BadHttpRequestException($"This account already has role {role}");
				}
				else if (Acc == null)
				{
					return false;
				}

				try
				{
					if (role == Role.TRAINEE)
					{
						if (campId == null)
						{
							return false;
						}
						await _userManager.AddToRoleAsync(Acc, role);
						Trainee Trainee;

						if (mentorId != null)
							Trainee = new Trainee() { UserId = Acc.Id, CampId = (int)campId, MentorId = mentorId };
						else
							Trainee = new Trainee() { UserId = Acc.Id, CampId = (int)campId };

						await Trainees.addAsync(Trainee);
					}
					else if (role == Role.MENTOR)
					{
						await _userManager.AddToRoleAsync(Acc, role);
						Mentor mentor = new Mentor() { UserId = Acc.Id};
						await Mentors.addAsync(mentor);
						await _dataBase.SaveChangesAsync();

						if(campId is not null)
						{
							var camp = await _dataBase.Camps.SingleAsync(c => c.Id == campId);
							mentor.Camps = new List<Camp>() { camp };
							_dataBase.Update(mentor);
						}
					}
					else if (role == Role.HOC && campId!=null)
					{
						await _userManager.AddToRoleAsync(Acc, role);
						HeadOfTraining HeadOfTraining = new HeadOfTraining() { UserId = Acc.Id, CampId = campId };
						await HeadofCamp.addAsync(HeadOfTraining);
					}
					else if (role == Role.LEADER || role == Role.INSTRUCTOR)
					{
						await _userManager.AddToRoleAsync(Acc, role);
					}
					else
					{
						return false;
					}
					return true;
				}
				catch
				{
					return false;
				}
			}
			else
			{
				return false;
			}
			

		}
		public async Task<int> completeAsync()
		{
			return await _dataBase.SaveChangesAsync();
		}
        public void Dispose()
		{
			_dataBase.Dispose();
		}
	}
}
