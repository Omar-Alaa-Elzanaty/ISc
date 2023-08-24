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
	public class MentorRepository : BaseRepository<Mentor>, IMentorRepository
	{
		private readonly DataBase _Context;
        private readonly UserManager<UserAccount> _UserManager;
        public MentorRepository(DataBase context,UserManager<UserAccount>usermanager):base(context)
        {
            _Context = context;
            _UserManager = usermanager;
        }
        public async Task<object> showMentorsAccountsAsync()
        {
            var Accounts=await _UserManager.GetUsersInRoleAsync(Roles.MENTOR);

            return (from account in Accounts
                   from mentor in _Context.Mentors
                   where mentor.UserId==account.Id
                   select new {account, mentor}).ToList();
        }
    }
}
