using AutoMapper;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Services.ISerivces;
using Microsoft.EntityFrameworkCore;

namespace ISC.Services.Services
{
    public class PublicSerivces : IPublicSerives
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOnlineJudgeServices _onlineJudgeServices;
        private readonly IMapper _mapper;
        private readonly IMediaServices _mediaServices;
        public PublicSerivces(
            IUnitOfWork unitOfWork,
            IOnlineJudgeServices onlineJudgeServices,
            IMapper mapper,
            IMediaServices mediaServices)
        {
            _unitOfWork = unitOfWork;
            _onlineJudgeServices = onlineJudgeServices;
            _mapper = mapper;
            _mediaServices = mediaServices;
        }
        public async Task<ServiceResponse<string>> AddNewRegister(NewRegisterationDto model)
        {
            var response = new ServiceResponse<string>();

            var campName = _unitOfWork.Camps.Get().Where(c => c.Id == model.CampId).Select(c => c.Name).First();

            if (_unitOfWork.NewRegitseration.Get()
                .Any(t => t.CampId == model.CampId && t.Email == model.Email))
            {
                response.Comment = "Email already exist";
                return response;
            }
            if (_unitOfWork.NewRegitseration.Get().Any(t => t.CampId == model.CampId && t.NationalID == model.NationalID))
            {
                response.Comment = "NationalId is already exist!";
                return response;
            }
            if (_unitOfWork.NewRegitseration.Get()
                .Any(t => t.CampId == model.CampId && t.PhoneNumber != null && t.PhoneNumber == model.PhoneNumber))
            {
                response.Comment = "Phone already exist";
                return response;
            }
            if (_unitOfWork.NewRegitseration.Get().Any(t => t.CampId == model.CampId && t.CodeForceHandle == model.CodeForceHandle))
            {
                response.Comment = "CodeForceHandle already exist!";
                return response;
            }
            if (_unitOfWork.NewRegitseration.Get()
                .Any(t => t.CampId == model.CampId && t.VjudgeHandle != null && t.VjudgeHandle == model.VjudgeHandle))
            {
                response.Comment = "VjudgeHandle already exist!";
                return response;
            }
            if (_unitOfWork.NewRegitseration.Get()
                .Any(t => t.CampId == model.CampId && t.FacebookLink != null && t.FacebookLink == model.FacebookLink))
            {
                response.Comment = "Facebook already exist";
                return response;
            }
            if (_unitOfWork.NewRegitseration.Get()
                .Any(t => t.CampId == model.CampId
                    && t.FirstName + t.MiddleName + t.LastName == model.FirstName + model.MiddleName + model.LastName))
            {
                response.Comment = "You already registerd before";
                return response;
            }
            if (await _onlineJudgeServices.checkHandleValidationAsync(model.CodeForceHandle))
            {
                response.Comment = "In correct CodeforceHandle";
                return response;
            }

            var isInTraineeArchive = await _unitOfWork.TraineesArchive.Get()
                                    .AnyAsync(t => t.CampName == campName &&
                                    (t.NationalID == model.NationalID ||
                                     t.CodeForceHandle == model.CodeForceHandle ||
                                    (t.VjudgeHandle != null && t.VjudgeHandle == model.VjudgeHandle) ||
                                    (t.FacebookLink != null && t.FacebookLink == model.FacebookLink) ||
                                    t.PhoneNumber == model.PhoneNumber ||
                                    t.Email == model.Email ||
                                    t.FirstName + t.MiddleName + t.LastName == model.FirstName + model.MiddleName + model.LastName));

            if (isInTraineeArchive)
            {
                response.Comment = "You are banned form registering for this camp";
                return response;
            }

            var newRegister = _mapper.Map<NewRegistration>(model);
            if (model.ImageFile is not null) newRegister.ImageUrl = await _mediaServices.AddAsync(model.ImageFile);

            await _unitOfWork.NewRegitseration.AddAsync(newRegister);
            _ = await _unitOfWork.completeAsync();

            response.IsSuccess = true;
            response.Entity = "Register Succcess Please Check your email from time to another in this week";

            return response;
        }
    }
}
