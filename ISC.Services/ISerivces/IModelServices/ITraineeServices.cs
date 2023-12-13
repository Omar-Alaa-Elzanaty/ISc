using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces.IModelServices
{
	public interface  ITraineeService
	{
		Task<ServiceResponse<object>> MentorInfoAsync(string traineeId);
		Task<string?> CampNameOfAsync(string traineeId);
		Task<List<object>> AccessSheetsWithMaterialsAsync(string userId);
		Task<object> GetTasks(string traineeId);
		Task UpdateTaskState(int taskId);
	}
}
