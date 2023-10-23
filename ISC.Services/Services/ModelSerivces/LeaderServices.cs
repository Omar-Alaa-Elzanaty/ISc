using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services.ModelSerivces
{
	public class LeaderServices:ILeaderServices
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<UserAccount> _userManager;
		public LeaderServices(IUnitOfWork unitOfWork, UserManager<UserAccount> userManager)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}
		public async Task<ServiceResponse<int>> DeleteTraineesAsync(List<string>traineesIds)
		{
			ServiceResponse<int> response = new ServiceResponse<int>() { Success = true };
			List<Trainee> trainees = new List<Trainee>();
			foreach(var traineeId in traineesIds)
			{
				var user =await _unitOfWork.Trainees.getByUserIdAsync(traineeId);
				if (user != null)
					trainees.Add(user);
			}
			if(trainees.Count == 0) {
				response.Success = false;
				response.Comment = "NO user Found";
				return response;
			}
			_unitOfWork.Trainees.deleteGroup(trainees);
			_=await _unitOfWork.comleteAsync();
			response.Entity = trainees.Count;
			return response;
		}
		public async Task<ServiceResponse<Camp>>AddCampAsync(CampDto camp)
		{
			ServiceResponse<Camp> response = new ServiceResponse<Camp>() { Success = true };
			var newCamp = new Camp()
			{
				Name = camp.Name,
				Term = camp.Term,
				Year = camp.Year,
				DurationInWeeks = camp.DurationInWeeks
			};
			_unitOfWork.Camps.addAsync(newCamp);
			int result =await _unitOfWork.comleteAsync();
			if(result == 0)
			{
				response.Success = false;
				response.Comment = "Couldn't add camp";
				return response;
			}
			response.Entity=newCamp; 
			return response;
		}
		public async Task<ServiceResponse<List<string>>> AddToRoleAsync(UserRoleDto model)
		{
			ServiceResponse<List<string>> response = new ServiceResponse<List<string>>();

			List<string> faillToAdd = new List<string>();
			foreach(var  userId in model.Users) {
				var user = await _userManager.FindByIdAsync(userId);
				bool isSuccess = await _unitOfWork.addToRoleAsync<UserAccount>(user, model.Role, model.CampId, model.MentorId);
				if(!isSuccess)
					faillToAdd.Add(user.FirstName+' '+user.MiddleName+' '+user.LastName);
			}
			if(faillToAdd.Count > 0)
			{
				response.Entity=faillToAdd;
				response.Comment = $"some users couldn't add to role {model.Role}";
				return response;
			}
			response.Success = true;
			return response;
		}
	}
}
