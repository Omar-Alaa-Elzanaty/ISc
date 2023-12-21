using AutoMapper;
using Azure;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services.ModelSerivces
{
	public class CampServices:ICampServices
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<UserAccount> _userManager;
		private readonly IMapper _mapper;
		public CampServices(IUnitOfWork unitOfWork,
			UserManager<UserAccount> userManager,
			IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_mapper = mapper;
		}
		public async Task<ServiceResponse<List<DisplayCampsDto>>> DisplayCampsDetails()
		{
			ServiceResponse<List<DisplayCampsDto>> response = new ServiceResponse<List<DisplayCampsDto>>();

			var campMentor = _mapper.Map<List<DisplayCampsDto>>(_unitOfWork.Camps.getAllAsync().Result.ToList());

			if (campMentor == null)
			{
				response.IsSuccess = false;
				response.Comment = "No camp found";
				return response;
			}

			foreach(var camp in campMentor)
			{
				var mentors = _unitOfWork.Mentors.Get()
					.Include(u => u.Camps)
					.Where(u => u.Camps.Any(m => m.Id == camp.Id))
					.Select(u=>u.Id)
					.ToListAsync();

				if (mentors != null && mentors.Result.Count() > 0) 
				{
					camp.Mentors.AddRange(await _userManager.Users
										.Include(u => u.Mentor)
										.Where(u => u.Mentor != null && mentors.Result.Any(j => j == u.Mentor.Id))
										.Select(u => u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName).ToListAsync());
				}

				var heads = await _unitOfWork.HeadofCamp
									.findManyWithChildAsync(hoc => hoc.CampId == camp.Id || hoc.CampId == null);

				if(heads.IsNullOrEmpty())
				{
					heads = new List<HeadOfTraining>();
				}

				foreach(var head in heads)
				{
					var acc = await _userManager.FindByIdAsync(head.UserId);

					if(acc is null) { continue; }
					
					var headName = acc.FirstName + ' ' + acc.MiddleName + ' ' + acc.LastName;
					camp.HeadsInfo.Add(new HeadInfo()
					{
						Name = headName,
						Id = head.Id,
						State = head.CampId != null
					});
				}
			}

			response.IsSuccess= true;
			response.Entity = campMentor;

			return response;
		}
	}
}
