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
	public  interface ILeaderServices
	{
		Task<ServiceResponse<int>> DeleteTraineesAsync(List<string> traineesIds);
		Task<ServiceResponse<Camp>> AddCampAsync(CampDto camp);
		Task<ServiceResponse<List<string>>> AddToRoleAsync(UserRoleDto model);
		Task<ServiceResponse<List<NewRegisterationDto>>> DisplayNewRegisterAsync(int campId);
		Task<ServiceResponse<AuthModel>> AutoMemberAddAsync(RegisterDto registerDto, string? message = null, string? campName = null);
		Task<ServiceResponse<bool>> DeleteFromNewRegister(List<string> Ids);


	}
}
