using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
    public class AccountRepository:BaseRepository<UserAccount>, IAccountRepository
	{
		private readonly DataBase _Context;
		private readonly UserManager<UserAccount> _UserManger;
        public AccountRepository(DataBase context, UserManager<UserAccount> usermanger) :base(context) 
        {
            _Context = context;
        }
		public async Task<bool> tryCreateTraineeAccountAsync(UserAccount account,string password)
		{
			var result = await _UserManger.CreateAsync(account, password);
			if (result.Succeeded)
			{
				Trainee NewTrainee = new Trainee(){UserId=account.Id};//wrong logic
				_Context.Trainees.Add(NewTrainee);
				_Context.SaveChanges();
				return true;
			}
			return false;
		}
	}
}
