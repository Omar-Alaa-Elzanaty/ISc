using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class CampRepository : BaseRepository<Camp>, ICampRepository
	{
		private readonly DataBase _Context;
		public CampRepository(DataBase context) : base(context)
		{
			_Context= context;
		}
		public async Task<Camp> getCampByUserIdAsync(string userid)
		{
			var CampId = _Context.HeadsOfTraining
				.Where(hoc => hoc.UserId == userid)
				.FirstOrDefaultAsync()
				.Result.CampId;
			return await _Context.Camps.FindAsync((int)CampId);
		}
		public async Task<string> GetNameByIdAsync(int id)
		{
			return _Context.Camps.FindAsync(id).Result.Name;
		}
	}
}
