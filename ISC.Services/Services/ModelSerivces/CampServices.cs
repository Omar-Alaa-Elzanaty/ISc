using AutoMapper;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISC.Services.Services.ModelSerivces
{
    public class CampServices : ICampServices
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
            await Task.CompletedTask;

            ServiceResponse<List<DisplayCampsDto>> response = new ServiceResponse<List<DisplayCampsDto>>()
            {
                Entity = new()
            };

            var camps = _unitOfWork.Camps.Get().Include(x => x.Heads).Include(x => x.Mentors).ToHashSet();

            if (camps == null)
            {
                response.IsSuccess = false;
                response.Comment = "No camp found";
                return response;
            }

            var mentors = _userManager.Users.Include(x => x.Mentor).Where(x => x.Mentor != null)
                .Select(x => new
                {
                    x.Id,
                    FullName = x.FirstName + ' ' + x.MiddleName + ' ' + x.LastName,
                    MentorId = x.Mentor!.Id,
                }).ToHashSet();

            var heads = _userManager.Users.Include(x => x.Headofcamp).Where(x => x.Headofcamp != null).
                Select(x => new
                {
                    x.Id,
                    FullName = x.FirstName + ' ' + x.MiddleName + ' ' + x.LastName,
                    HeadId = x.Headofcamp!.Id
                }).ToHashSet();


            foreach (var camp in camps)
            {
                var campResponse = _mapper.Map<DisplayCampsDto>(camp);

                campResponse.HeadsInfo.AddRange(heads.Select(x => new Member()
                {
                    Id = x.HeadId,
                    Name = x.FullName,
                    State = camp.Heads.Any(y => y.Id == x.HeadId)
                }));
                campResponse.CampMentors.AddRange(mentors.Select(x => new Member()
                {
                    Id = x.MentorId,
                    Name = x.FullName,
                    State = camp.Mentors.Any(y => y.Id == x.MentorId)
                }));

                response.Entity.Add(campResponse);
            }

            response.IsSuccess = true;

            return response;
        }
        public async Task<ServiceResponse<bool>> UpdateHeadAsync(int id, int? campId)
        {
            var head = await _unitOfWork.HeadofCamp.getByIdAsync(id);
            head.CampId = campId;

            await _unitOfWork.HeadofCamp.UpdateAsync(head);
            _ = await _unitOfWork.completeAsync();

            return new ServiceResponse<bool>() { IsSuccess = true };
        }
        public async Task<ServiceResponse<bool>> UpdateMentorAsync(int id, int campId, bool isAdd)
        {
            var mentor = await _unitOfWork.Mentors.Get().Include(x=>x.Camps).FirstAsync(x => x.Id == id);
            var camp = await _unitOfWork.Camps.getByIdAsync(campId);

            if (isAdd && !mentor.Camps.Contains(camp)) 
            {
                mentor.Camps.Add(camp);
            }
            else if (!isAdd && mentor.Camps.Contains(camp))
            {
                mentor.Camps.Remove(camp);
            }

            await _unitOfWork.Mentors.UpdateAsync(mentor);
            _ = await _unitOfWork.completeAsync();

            return new ServiceResponse<bool>() { IsSuccess = true };
        }
    }
}
