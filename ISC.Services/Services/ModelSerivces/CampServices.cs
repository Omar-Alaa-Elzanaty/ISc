using Azure;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services.ModelSerivces
{
	public class CampServices:ICampServices
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<UserAccount> _userManager;
		private readonly IOnlineJudgeServices _onlineJudgeServices;
		private readonly IAuthanticationServices _authServices;
		private readonly IMailServices _mailServices;
		private readonly DefaultMessages _defaultMessages;
		public CampServices(IUnitOfWork unitOfWork,
			UserManager<UserAccount> userManager,
			IOnlineJudgeServices onlineJudgeServices,
			IAuthanticationServices authanticationServices,
			IMailServices mailServices,
			IOptions<DefaultMessages> messages)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_onlineJudgeServices = onlineJudgeServices;
			_authServices = authanticationServices;
			_mailServices = mailServices;
			_defaultMessages = messages.Value;
		}
		public async Task<ServiceResponse<List<DisplayCampsDto>>> CampMentors()
		{
			ServiceResponse<List<DisplayCampsDto>> response = new ServiceResponse<List<DisplayCampsDto>>();

			var campMentor = _unitOfWork.Camps.getAllAsync().Result.Select(c => new DisplayCampsDto()
			{
				Id = c.Id,
				Name = c.Name,
				Mentors=new List<string>()
			}).ToList();

			foreach(var camp in campMentor)
			{
				var mentors = _unitOfWork.Mentors.Get()
					.Include(u => u.Camps)
					.Where(u => u.Camps.Any(m => m.Id == camp.Id))
					.Select(i => i.Id);

				camp.Mentors.AddRange(_userManager.Users
					.Include(u => u.Mentor)
					.Where(u => mentors.Contains(u.Mentor.Id))
					.Select(u => u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName).ToList());
			}

			response.Success= true;
			response.Entity = campMentor;

			return response;
		}
	}
}
