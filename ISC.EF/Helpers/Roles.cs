using ISC.Core.Interfaces;
using ISC.EF;
using ISC.EF.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Roles
	{
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IUnitOfWork _UnitOfWork;
        public Roles(UserManager<UserAccount> userManager,IUnitOfWork unitofwork)
        {
            _UserManager = userManager;
			_UnitOfWork= unitofwork;
        }
        public const string LEADER = "Leader";
		public const string TRAINEE = "Trainee";
		public const string MENTOR = "Mentor";
		public const string HOC = "Head of Training";
		public const string INSTRUCTOR = "Instructor";
		public async Task<bool> addToRoleAsync(UserAccount account, string role,dynamic type)
		{
			try
			{
				if (role == TRAINEE)
				{
					if(type.CampId==null) {
						var Account = _UserManager.DeleteAsync(account);
						return false;
					}
					await _UserManager.AddToRoleAsync(account, role);
					Trainee Trainee;
					if (type.MentorId != null)
						Trainee = new Trainee() { UserId = account.Id, CampId = type.CampId, MentorId = type.MentorId };
					else
						Trainee = new Trainee() { UserId = account.Id, CampId = type.CampId };
					_UnitOfWork.Trainees.addAsync(Trainee);
					return true;
				}
				else if (role == MENTOR)
				{
					await _UserManager.AddToRoleAsync(account, role);
					Mentor Mentor = new Mentor() { UserId = account.Id };
					_UnitOfWork.Mentors.addAsync(Mentor);
					return true;
				}
				else if (role == HOC)
				{
					await _UserManager.AddToRoleAsync(account, role);
					if (type.CampId != null)
					{
						HeadOfTraining HeadOfTraining = new HeadOfTraining() { UserId = account.Id, CampId = type.CampId };
						_UnitOfWork.HeadofCamp.addAsync(HeadOfTraining);
					}
					return true;
				}
				else {
					if (role == LEADER || role == INSTRUCTOR)
					{
						await _UserManager.AddToRoleAsync(account, role);
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			catch
			{
				return false;
			}
			
		}
	}
}
