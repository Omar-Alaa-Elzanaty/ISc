using static ISC.API.APIDtos.CodeForcesDtos;

namespace ISC.API.ISerivces
{
	public interface IOnlineJudgeServices
	{
		Task<bool> checkHandleValidationAsync(string handle);
		Task<CodeforcesApiResponseDto<CodeforceStandingResult>> getContestStandingAsync(string contestid, int numberofrows,bool unofficial);
	}
}
