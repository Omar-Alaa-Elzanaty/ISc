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
            var Accounts=await _UserManager.GetUsersInRoleAsync(Role.MENTOR);

            return (from account in Accounts
                   from mentor in _Context.Mentors
                   where mentor.UserId==account.Id
                   select new {account, mentor}).ToList();
        }
        public  async Task<bool> deleteAsync(Mentor mentor)
        {
			if (mentor == null) return false;
			int Trainees = await _Context.Trainees.Where(t=>t.MentorId==mentor.Id).CountAsync();
			if (Trainees != 0)
				return false;
			try
			{
				_Context.Remove(mentor);
				return true;
			}
			catch
			{
				return false;
			}
		}
        public async Task<bool> deleteAsync(string userid)
        {

            var mentor = await _Context.Mentors.Where(x => x.UserId == userid).FirstOrDefaultAsync();

            if(mentor is null || await _Context.Trainees.AnyAsync(x => x.MentorId == mentor.Id))
            {
                return false;
            }

            
            bool Result =await base.deleteAsync(mentor);

            return Result;
		}
	}
}
