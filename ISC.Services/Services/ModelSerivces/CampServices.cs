using Azure;
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
		public async Task<ServiceResponse<object>> CampMentors()
		{
			ServiceResponse<object> response = new ServiceResponse<object>();

			int camp = 0;
			if(camp==null)
			{
				response.Success= false;
				response.Comment = "NO Data";
				return response;
			}
			var campMentor = _unitOfWork.Camps.FindWithMany(new[] { "Mentors" }).Result.Select(c => new
			{
				c.Id,
				c.Name,
				Mentors = c.Mentors.Select(m => new
				{
					Id = m.UserId,
				}).Join(_userManager.Users,
						m => m.Id,
						u => u.Id,
						(m, u) => new
						{
							FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName
						}).ToList()
				.ToList()
			});
			response.Success= true;
			response.Entity = campMentor;
			return response;
		}
	}
}
