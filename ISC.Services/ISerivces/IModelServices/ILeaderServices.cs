using ISC.Core.APIDtos;
using ISC.Core.Dtos;
using ISC.Core.Models;
using ISC.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces.IModelServices
{
	public  interface ILeaderServices
	{
		Task<ServiceResponse<Camp>> AddCampAsync(CampDto camp);
		Task<ServiceResponse<List<string>>> AddToRoleAsync(UserRoleDto model);
		Task<ServiceResponse<object>> DisplayNewRegisterAsync(int campId);
		Task<ServiceResponse<AuthModel>> AutoMemberAddAsync(RegisterDto registerDto, string? message = null, string? campName = null);
		Task<ServiceResponse<bool>> DeleteFromNewRegister(List<string> Ids);
		Task<ServiceResponse<bool>> DeleteTraineesAsync(List<DeleteTraineeDto> trainees);
		Task<ServiceResponse<bool>> AssignRoleToStuff(StuffNewRolesDto model);
		Task<ServiceResponse<List<UserAccount>>> DeleteStuffAsync(List<string> StuffsIds);
		Task<ServiceResponse<string>> DeleteTraineeArchivesAsync(List<string> trainees);
		Task<ServiceResponse<bool>> UpdateTraineeArchive(HashSet<TraineeArchiveDto> archives);
		Task<ServiceResponse<bool>> UpdateStuffArchive(HashSet<StuffArchiveDto> archives);
		Task<ServiceResponse<AuthModel>> SubmitNewRegister(SubmitNewRegisterDto newRegisters);
		Task<ServiceResponse<int>> UpdateCampStatusAsync(int campId);

    }
}
