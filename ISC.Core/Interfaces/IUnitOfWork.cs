using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface IUnitOfWork:IDisposable
	{
		ITraineeRepository Trainees { get; }
		IBaseRepository<Session> Sessions { get; }
		IMentorRepository Mentors { get; }
		IBaseRepository<TraineeAttendence> TraineesAttendence { get; }
		IBaseRepository<Sheet> Sheets { get; }
		IBaseRepository<TraineeSheetAccess> TraineesSheetsAccess { get; }
		IHeadOfCampRepository HeadofCamp { get; }
		IBaseRepository<Camp> Camps { get; }
		ISessionFeedbackRepository SessionsFeedbacks { get; }
		IBaseRepository<MentorOfCamp> MentorsOfCamps { get; }
		IBaseRepository<Material> Materials { get; }
		IBaseRepository<TraineeArchive> TraineesArchive { get; }
		IBaseRepository<StuffArchive> StuffArchive { get; }
		IBaseRepository<NewRegitseration> NewRegitseration { get; }

		Task<int> comleteAsync();
	}
}
