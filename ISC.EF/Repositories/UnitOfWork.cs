using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
    public class UnitOfWork : IUnitOfWork
	{
		private readonly UserManager<UserAccount> _UserManager;
		private readonly DataBase _DataBase;
		public IBaseRepository<Trainee> Trainees { get; private set; }

		public IBaseRepository<Session> Sessions { get; private set; }

		public IMentorRepository Mentors { get; private set; }

		public IBaseRepository<TraineeAttendence> TraineesAttendence { get; private set; }

		public IBaseRepository<Sheet> Sheets { get; private set; }

		public IBaseRepository<TraineeSheetAccess> TraineesSheetsAccess { get; private set; }

		public IBaseRepository<HeadOfTraining> HeadofCamp { get; private set; }

		public IBaseRepository<Camp> Camps { get; private set; }

		public IBaseRepository<SessionFeedback> SessionsFeedback { get; private set; }

		public IBaseRepository<MentorOfCamp> MentorsOfCamps { get; private set; }

		public IBaseRepository<Material> Materials { get; private set; }

		public IBaseRepository<TraineeArchive> TraineesArchive { get; private set; }

		public IBaseRepository<StuffArchive> StuffArchive { get; private set; }

		public IBaseRepository<NewRegitseration> NewRegitseration { get; private set; }
        public UnitOfWork(DataBase database,UserManager<UserAccount>usermanager)
        {
			_DataBase = database;
			_UserManager = usermanager;

			Trainees = new BaseRepository<Trainee>(_DataBase);
			Sessions= new BaseRepository<Session>(_DataBase);
			Mentors = new MentorRepository(database,usermanager);
			TraineesAttendence = new BaseRepository<TraineeAttendence>(_DataBase);
			Sheets = new BaseRepository<Sheet>(_DataBase);
			TraineesSheetsAccess = new BaseRepository<TraineeSheetAccess>(_DataBase);
			HeadofCamp = new BaseRepository<HeadOfTraining>(_DataBase);
			Camps = new BaseRepository<Camp>(_DataBase);
			SessionsFeedback = new BaseRepository<SessionFeedback>(_DataBase);
			MentorsOfCamps = new BaseRepository<MentorOfCamp>(_DataBase);
			Materials = new BaseRepository<Material>(_DataBase);
			TraineesArchive = new BaseRepository<TraineeArchive>(_DataBase);
			StuffArchive = new BaseRepository<StuffArchive>(_DataBase);
			NewRegitseration = new BaseRepository<NewRegitseration>(_DataBase);
		}
		public int comlete()
		{
			return _DataBase.SaveChanges();
		}
        public void Dispose()
		{
			_DataBase.Dispose();
		}
	}
}
