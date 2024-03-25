using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;

namespace ISC.Services.Services.ModelSerivces
{
    public class MentorSerivces : IMentorServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        public MentorSerivces(
            IUnitOfWork unitOfWork,
            UserManager<UserAccount> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<List<DisplayCampsForMentorDto>>> GetCampsNames(string userId)
        {
            var response = new ServiceResponse<List<DisplayCampsForMentorDto>>();

            var camps =  _unitOfWork.Mentors.Get().Include(x=>x.Camps).SingleOrDefaultAsync(x=>x.UserId==userId).Result?.Camps;

            if(camps is null)
            {
                response.Comment = "mentor not found or doesn't partipate in any camp";
                return response;
            }

            response.Entity = camps.Select(x => new DisplayCampsForMentorDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            response.IsSuccess = true;
            return response;
        }

        public async Task<ServiceResponse<MentorInfoDto>?> MentorInfoAsync(string userId)
        {
            var response = new ServiceResponse<MentorInfoDto>();

            var user = await _userManager.FindByIdAsync(userId);

            if(user is null)
            {
                response.Comment = "User not found.";
                return response;
            }

            var mentorInfo = await _unitOfWork.Mentors.Get().Include(x=>x.Camps).SingleOrDefaultAsync(x => x.UserId == userId);

            if(mentorInfo is null)
            {
                response.Comment = "Mentor info not found.";
                return response;
            }

            response.IsSuccess = true;
            response.Entity = new()
            {
                Id = mentorInfo.Id,
                FullName = user.FirstName + ' ' + user.MiddleName + ' ' + user.LastName,
                ProfileImageUrl = user.PhotoUrl,
                Camps = mentorInfo.Camps.Select(x => x.Name).ToList(),
                CodeforceHandle = user.CodeForceHandle,
                Collage = user.College,
                Email = user.Email!,
                FacebookLink = user.FacebookLink,
                Gander = user.Gender,
                PhoneNumber = user.PhoneNumber,
                VjudgeHandle = user.VjudgeHandle
            };

            return response;
        }
    }
}
