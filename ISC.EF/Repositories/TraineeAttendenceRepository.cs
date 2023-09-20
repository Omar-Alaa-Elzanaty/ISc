using ISC.Core.Interfaces;
using ISC.Core.ModelsDtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class TraineeAttendenceRepository : BaseRepository<TraineeAttendence>, ITraineeAttendenceRepository
	{
		private readonly DataBase _DataBase;
		public TraineeAttendenceRepository(DataBase context) : base(context)
		{
			_DataBase = context;
		}
		public async Task<List<TraineeAttendence>> getInListAsync(Expression<Func<TraineeAttendence,bool>>match)
		{
			return await _DataBase.TraineesAttednces.Where(match).ToListAsync();
		}
	}
}
