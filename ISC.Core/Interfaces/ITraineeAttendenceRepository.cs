using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface ITraineeAttendenceRepository:IBaseRepository<TraineeAttendence>
	{
		Task<List<TraineeAttendence>> getInListAsync(Expression<Func<TraineeAttendence, bool>> match);
	}
}
