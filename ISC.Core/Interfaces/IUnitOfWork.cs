using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface IUnitOfWork:IDisposable
	{ 
		ITraineeRepository Trainees { get; }
		ISessionRepository Sessions { get; }
		IMentorRepository Mentors { get; }
		ITraineeAttendenceRepository TraineesAttendence { get; }
		IBaseRepository<MentorAttendence> MentorAttendence { get; }
		ISheetRepository Sheets { get; }
		IBaseRepository<TraineeSheetAccess> TraineesSheetsAccess { get; }
		IHeadOfCampRepository HeadofCamp { get; }
		ICampRepository Camps { get; }
		ISessionFeedbackRepository SessionsFeedbacks { get; }
		IBaseRepository<Material> Materials { get; }
		IBaseRepository<TraineeArchive> TraineesArchive { get; }
		IBaseRepository<StuffArchive> StuffArchive { get; }
		IBaseRepository<NewRegistration> NewRegitseration { get; }
		Task<bool> addToRoleAsync<T>(T account, string role,int?CampId,int?MentorId);
		Task<string?> getMediaAsync(IFormFile media);
		Task<int> completeAsync();
	}
}
