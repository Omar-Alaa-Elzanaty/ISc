using Azure;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using static ISC.Core.APIDtos.CodeForcesDtos;

namespace ISC.Services.ISerivces
{
	public interface IOnlineJudgeServices
	{
		Task<bool> checkHandleValidationAsync(string handle);
		Task<CodeforcesApiResponseDto<CodeforceStandingResultDto>> GetContestStandingAsync(string contestid, int numberofrows, bool unofficial, string apikey, string apisecret);
		Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> GetUserStatusAsync(string apikey, string apisecret);
		Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> GetContestStatusAsync(string contestid, string apikey, string apisecret,int count, string? handle=null);
	}
}
