using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.AspNetCore.Identity;
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

	}
}
