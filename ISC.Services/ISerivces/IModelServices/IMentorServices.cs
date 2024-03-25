using ISC.Core.Dtos;
using ISC.Core.Models;

namespace ISC.Services.ISerivces.IModelServices
{
    public interface IMentorServices
    {
        Task<ServiceResponse<List<DisplayCampsForMentorDto>>> GetCampsNames(string userId);
        Task<ServiceResponse<MentorInfoDto>?> MentorInfoAsync(string userId);
    }
}
