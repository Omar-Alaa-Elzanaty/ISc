using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface ITraineeRepository:IBaseRepository<Trainee>
	{
		 Task<Trainee?> GetByUserIdAsync(string userId);
		Task<Camp?> GetCampOfTrainee(int id);
		Task<Camp?> GetCampOfTrainee(string userId);
	}
}
