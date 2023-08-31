using static ISC.API.APIDtos.CodeForcesDtos;

namespace ISC.API.ISerivces
{
	public interface IOnlineJudgeServices
	{
		Task<bool> checkHandleValidationAsync(string handle);
		Task<CodeforcesApiResponseDto<CodeforceStandingResultDto>> getContestStandingAsync(string contestid, int numberofrows,bool unofficial);
		Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getUserStatusAsync();
		Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getContestStatusAsync(string contestid,string handle="");
	}
}
