using ISC.Core.APIDtos;
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
		Task<ServiceResponse<bool>> SubmitTraineeMentorAsync(List<AssignTraineeMentorDto> data);
		Task<ServiceResponse<TraineeSheetAcessDto>> DisplayTraineeAccess(int campId);
		Task AddNewTrainingSheetAccess(int sheetId, int campId);
		Task<List<TraineeStandingDto>> GeneralStandingsAsync(int? campId);
		Task<PersonAttendenceDto> TraineeAttendence(int? campId);
		Task<PersonAttendenceDto> MentorAttendence(int? campId);
		Task<ServiceResponse<object>> DisplayTrainees(string userId);
		Task<ServiceResponse<bool>> DeleteFromTrianee(List<string> traineesUsersId);
		Task<ServiceResponse<List<KeyValuePair<FilteredUserDto, string>>>> WeeklyFilterAsync(List<string> selectedTrainees, string headId);
		Task<ServiceResponse<object>> SubmitWeeklyFilterAsync(List<string> traineesUsersId, string headUserId);
		Task<ServiceResponse<List<object>>> DisplayMentorsAsync(string userId);
		Task<ServiceResponse<List<Session>>> DisplaySessionsAsync(string userId);
		Task<ServiceResponse<int>> AddSessionAsync(SessionDto model, string userId);
		Task<ServiceResponse<int>> DeleteSessionAsync(int id);
    }
}
