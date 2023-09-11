using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class TraineeRepository:BaseRepository<Trainee>,ITraineeRepository
	{
		private readonly DataBase _Context;
		private readonly UserManager<UserAccount> _UserManager;
		public TraineeRepository(DataBase context, UserManager<UserAccount> usermanager) : base(context)
		{
			_Context = context;
			_UserManager = usermanager;
		}
		public async Task<Trainee>getByUserIdAsync(string userid)
		{
			return await _Context.Trainees
								 .Where(trainee => trainee.UserId == userid)
								 .SingleOrDefaultAsync();
		}
		public async Task<Camp>getCampofTrainee(int id)
		{
			return await _Context.Trainees
								 .Include(tr => tr.Camp)
								 .Where(tr => tr.Id == id)
								 .Select(tr=>tr.Camp).SingleOrDefaultAsync();
		}
	}
}
