using ISC.Core.Dtos;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces.IModelServices
{
    public interface IHeadSerivces
    {
		Task<ServiceResponse<List<TraineeMentorDto>>> DisplayTraineeMentorAsync(string userId);
		Task SubmitTraineeMentorAsync(List<AssignTraineeMentorDto> data);
		Task<ServiceResponse<TraineeSheetAcessDto>> DisplayTraineeAccess(int campId);
		Task AddNewTrainingSheetAccess(int sheetId, int campId);
		Task<List<TraineeStandingDto>> GeneralStandingsAsync(int? campId);
		Task<PersonAttendenceDto> TraineeAttendence(int? campId);
		Task<PersonAttendenceDto> MentorAttendence(int? campId);
		Task<ServiceResponse<object>> DisplayTrainees(string userId);
		Task<ServiceResponse<bool>> DeleteFromTrianee(List<string> traineesUsersId);

    }
}
