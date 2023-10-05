using static ISC.Core.APIDtos.CodeForcesDtos;

namespace ISC.Services.ISerivces
{
	public interface IOnlineJudgeServices
	{
		Task<bool> checkHandleValidationAsync(string handle);
		Task<CodeforcesApiResponseDto<CodeforceStandingResultDto>> getContestStandingAsync(string contestid, int numberofrows, bool unofficial, string apikey, string apisecret);
		Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getUserStatusAsync(string apikey, string apisecret);
		Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getContestStatusAsync(string contestid,string handle,string apikey, string apisecret);
	}
}
