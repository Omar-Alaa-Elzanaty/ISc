using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services.ModelSerivces
{
    public class HeadServices:IHeadSerivces
    {
		private readonly UserManager<UserAccount> _UserManager;
		private readonly IUnitOfWork _UnitOfWork;

		public HeadServices(UserManager<UserAccount> userManager,
			IUnitOfWork unitofwork)
		{
			_UserManager = userManager;
			_UnitOfWork = unitofwork;
		}

		public async Task<ServiceResponse<List<TraineeMentorDto>>> DisplayTraineeMentorAsync(string userId)
		{
			var response = new ServiceResponse<List<TraineeMentorDto>>();

			var campId = _UnitOfWork.HeadofCamp.findWithChildAsync(h => h.UserId == userId, new[] { "Camp" }).Result?.CampId;

			var trainees =await _UserManager.Users
				.Include(u => u.Trainee)
				.Where(u => u.Trainee != null && u.Trainee.CampId == campId)
				.ToListAsync();

			var mentors = await _UserManager.Users
				.Include(u => u.Mentor)
				.Where(u => u.Mentor != null)
				.ToListAsync();

			var traineeMentor = trainees.Join(mentors, t => t.Trainee.MentorId, m => m.Mentor.Id,
				(t, m) => new TraineeMentorDto()
				{
					TraineeId = t.Trainee.Id,
					MentorId = m.Mentor?.Id,
					TraineeName = t.FirstName + ' ' + t.MiddleName + " " + t.LastName,
					MentorName = m.Mentor != null ? m.FirstName + ' ' + m.MiddleName + " " + m.LastName : null
				}).ToList();

			response.Success = true;
			response.Entity = traineeMentor;

			if (traineeMentor.IsNullOrEmpty())
			{
				response.Entity = new List<TraineeMentorDto>();
			}

			return response;
		}
		public async Task SubmitTraineeMentorAsync(List<AssignTraineeMentorDto> data)
		{
			foreach(var item in data)
			{
				var trainee = await _UnitOfWork.Trainees.getByIdAsync(item.TraineeId);
				if(trainee is null)
				{
					continue;
				}
				var mentor = await _UnitOfWork.Mentors.getByIdAsync(item.MentorId);
				trainee.Mentor= mentor;
				trainee.MentorId = item.MentorId;
				await _UnitOfWork.Trainees.UpdateAsync(trainee);
			}
			await _UnitOfWork.completeAsync();
		}
	}
}
