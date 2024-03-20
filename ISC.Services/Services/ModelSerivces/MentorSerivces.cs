using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.EntityFrameworkCore;

namespace ISC.Services.Services.ModelSerivces
{
    public class MentorSerivces : IMentorServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public MentorSerivces(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<List<DisplayCampsForMentorDto>>> GetCampsNames(string userId)
        {
            var response = new ServiceResponse<List<DisplayCampsForMentorDto>>();

            var mentor = await _unitOfWork.Mentors.Get().Include(x => x.Camps).FirstOrDefaultAsync(x => x.UserId == userId);

            if(mentor is null)
            {
                response.Comment = "mentor not found";
                return response;
            }

            response.Entity = mentor.Camps.Select(x => new DisplayCampsForMentorDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            return response;
        }
    }
}
