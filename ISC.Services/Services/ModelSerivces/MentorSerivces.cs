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

            return response;
        }
    }
}
