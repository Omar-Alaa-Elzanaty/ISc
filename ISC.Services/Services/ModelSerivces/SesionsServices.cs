using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.Services.ISerivces.IModelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services.ModelSerivces
{
	public class SesionsServices : ISessionsServices
	{
		private readonly IUnitOfWork _unitOfWork;
        public SesionsServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ServiceResponse<HashSet<int>>> SessionFilter(List<int> traineesIds)
		{
			var traineeAttend = _unitOfWork.TraineesAttendence
					.getInListAsync(tr => traineesIds.Contains(tr.TraineeId))
								.Result.GroupBy(attend => attend.TraineeId)
								.Select(g => new { TraineeId = g.Key, NumberOfAttendence = g.Count() }).ToList();
			HashSet<int> filteredOnSessions = new HashSet<int>();
			ServiceResponse<HashSet<int>>response=new ServiceResponse<HashSet<int>>();
			if (traineeAttend.Count > 0)
			{
				int MaxAttendence = traineeAttend.Max(ta => ta.NumberOfAttendence);
				filteredOnSessions = traineeAttend
								.Where(i => MaxAttendence - i.NumberOfAttendence > 2)
								.Select(ta => ta.TraineeId).ToHashSet();
			}
			if (filteredOnSessions.Count == 0)
			{
				response.Success = false;
				response.Comment="Empty filtered list";
				return response;
			}
			response.Success = true;
			response.Entity = filteredOnSessions;
			return response;
		}
	}
}
