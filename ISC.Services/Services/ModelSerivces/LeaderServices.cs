using ISC.Core.APIDtos;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.EF.Repositories;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
		private readonly IOnlineJudgeServices _onlineJudgeServices;
		private readonly IAuthanticationServices _authServices;
		public LeaderServices(IUnitOfWork unitOfWork,
			UserManager<UserAccount> userManager,
			IOnlineJudgeServices onlineJudgeServices,
			IAuthanticationServices authanticationServices)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_onlineJudgeServices = onlineJudgeServices;
			_authServices = authanticationServices;
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
			_=await _unitOfWork.completeAsync();
			response.Entity = trainees.Count;
			return response;
		}
		public async Task<ServiceResponse<Camp>>AddCampAsync(CampDto camp)
		{
			ServiceResponse<Camp> response = new ServiceResponse<Camp>() { Success = true };
			var campItem =await _unitOfWork.Camps.findByAsync(c => c.Name == camp.Name);
			if(campItem is not null)
			{
				response.Success = false;
				response.Comment = "Camp found before";
				return response;
			}
			var newCamp = new Camp()
			{
				Name = camp.Name,
				Term = camp.Term,
				Year = camp.Year,
				DurationInWeeks = camp.DurationInWeeks
			};
			await _unitOfWork.Camps.addAsync(newCamp);
			int result =await _unitOfWork.completeAsync();
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
		public async Task<ServiceResponse<List<NewRegisterationDto>>> DisplayNewRegisterAsync()
		{
			ServiceResponse<List<NewRegisterationDto>> response = new ServiceResponse<List<NewRegisterationDto>>();
			List<NewRegisterationDto> filter = new List<NewRegisterationDto>();
			var traineeArchive = await _unitOfWork.TraineesArchive.getAllAsync();
			foreach (var newMember in await _unitOfWork.NewRegitseration.getAllAsync())
			{
				var register = new NewRegisterationDto()
				{
					Register = newMember
				};

				if ( traineeArchive!=null && traineeArchive?
					.Any(TA => (TA.NationalID == newMember.NationalID
							   || TA.CodeForceHandle == newMember.CodeForceHandle
							   || TA.Email == newMember.Email
							   || (newMember.FacebookLink != null && newMember.FacebookLink == TA.FacebookLink)
							   || (newMember.PhoneNumber != null && newMember.PhoneNumber == TA.PhoneNumber))
							&& TA.CampName.ToLower() == newMember.CampName.ToLower()) == true)
				{
					register.State = "Archive";
				}
				else if(true/*await _onlineJudgeServices.checkHandleValidationAsync(newMember.CodeForceHandle)*/)
				{
					register.State = "New";
				}

				filter.Add(register);
			}
			if(filter.Count > 0)
			{
				response.Success = true;
				response.Entity = filter;
			}
			return response;
		}
		public async Task<ServiceResponse<AuthModel>>AutoMemberAddAsync(RegisterDto registerInfo,string? message = null,string?campName = null)
		{
			ServiceResponse<AuthModel> response = new ServiceResponse<AuthModel>();
			AuthModel result = await _authServices.RegisterAsync(
				user: registerInfo,
				message: message,
				sendEmail: true
				);
			if (!result.IsAuthenticated)
			{
				response.Success = false;
				response.Comment = "Couldn't create account";

				return response;
			}

			response.Success=true;

			response.Entity = result;

			return response;
		}
		public async Task<ServiceResponse<bool>> DeleteFromNewRegister(List<string>Ids)
		{
			var response =new ServiceResponse<bool>();
			var registers =await _unitOfWork.NewRegitseration.findManyWithChildAsync(r => Ids.Contains(r.NationalID));
			if (registers != null||registers.Count==0) {
				response.Comment = "No data found";
				return response;
			}
			_unitOfWork.NewRegitseration.deleteGroup(registers);
			response.Success = true;
			return response;
		}
	}
}
