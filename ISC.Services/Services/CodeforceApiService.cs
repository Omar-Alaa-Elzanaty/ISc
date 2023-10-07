using Azure;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using static ISC.Core.APIDtos.CodeForcesDtos;

namespace ISC.Services.Services
{
	public class CodeforceApiService : IOnlineJudgeServices
	{
		private readonly CodeForceConnection _cfConnection;
		private readonly ApiRequestServices _apiRequest;
		public CodeforceApiService(IOptions<CodeForceConnection>cfconnection)
		{
			_cfConnection = cfconnection.Value;
			_apiRequest = new ApiRequestServices("https://codeforces.com/api/");
		}
		public async Task<bool> checkHandleValidationAsync(string handle)
		{
			try
			{
				var Response = await _apiRequest
					.getRequestAsync<CodeforcesApiResponseDto<List<CodeforcesUserDto>>>($"user.info?handles={handle}");
				if (Response == null) return false;
				var ResponseContent =(CodeforcesApiResponseDto<List<CodeforcesUserDto>>) Response;
				return !(ResponseContent.status != "OK" || ResponseContent.result.Count == 0);
			}
			catch 
			{
				return false;
			}
		}
		public async Task<CodeforcesApiResponseDto<CodeforceStandingResultDto>> GetContestStandingAsync(string contestid,int numberofrows,bool unofficial,string apikey,string apisecret)
		{
			try//377686
			{

				string request = "contest.standings?"+generatecontestStandingRequest(contestid, numberofrows,unofficial,apikey,apisecret);
				var Response = await _apiRequest.getRequestAsync<CodeforcesApiResponseDto<CodeforceStandingResultDto>>(request);
				var Standing= (CodeforcesApiResponseDto<CodeforceStandingResultDto>)Response;
				if (Standing == null) return null;
				else return Standing;
			}
			catch
			{
				return null;
			}
		}
		public async Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> GetUserStatusAsync(string apikey,string apisecret)
		{
			try//377686
			{

				string request = "user.status?" + generateUserStatusRequest(apikey,apisecret);
				var Response = await _apiRequest.getRequestAsync<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>(request);
				var UserStatus = (CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>)Response;
				if (UserStatus == null) return null;
				else return UserStatus;
			}
			catch
			{
				return null;
			}
		}
		public async Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> GetContestStatusAsync(string contestid,string handle,string apikey,string apisecret)
		{
			try//377686
			{
				Console.WriteLine("test okay ");
				string request = "contest.status?"+generateContestStatusRequest(contestid,handle,apikey,apisecret);
				var Response = await _apiRequest.getRequestAsync<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>(request);
				var ContestStatus = (CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>)Response;
				if (ContestStatus == null) return null;
				else return ContestStatus;
			}
			catch
			{
				return null;
			}
		}

		public async Task<ResponseModel<Dictionary<int, int>>> SheetsProblemsCount(List<TraineeSheetAccess>traineesAcces)
		{
			ResponseModel<Dictionary<int, int>> response = new ResponseModel<Dictionary<int, int>>();
			Dictionary<int, int> ProblemSheetCount = traineesAcces
				.DistinctBy(tsa => tsa.SheetId)
				.Select(i => new {
					i.SheetId,
					Count = GetContestStandingAsync(i.Sheet.SheetCfId, 1, true,
							i.Sheet.IsSohag ? _cfConnection.SohagKey : _cfConnection.AssuitKey
							, i.Sheet.IsSohag ? _cfConnection.SohagSecret : _cfConnection.AssuitSecret)
				.Result.result.problems.Count()
				})
				.ToDictionary(i => i.SheetId, i => i.Count);
			if(ProblemSheetCount.Count > 0)
			{
				response.State = true;
				response.Entity=ProblemSheetCount;
				return response;
			}
			response.State = false;
			response.Comment = "Coudn't find sheets or problems of sheets";
			return response;
		}
		private string generatecontestStandingRequest(string contestid,int numberofrows,bool unofficial,string apikey,string apisecret)
		{
			string Parameters = "";
			Parameters += addParameter("apiKey", apikey);
			Parameters += addParameter("contestId", contestid);
			Parameters += addParameter("count", Math.Max(numberofrows,1).ToString());
			Parameters += addParameter("from", "1");
			Parameters += addParameter("showUnofficial", unofficial==true?"True":"false");
			Parameters += addParameter("time", new Converters().generateTimeInUnix().ToString());
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			var ApiSig = generateSig(Parameters, "/contest.standings?",apisecret);
			if (ApiSig == null) return null;
			Parameters += "&";
			Parameters += addParameter("apiSig", ApiSig);
			Parameters=Parameters.Substring(0,Parameters.Length - 1);
			return  Parameters;
		}
		private string generateUserStatusRequest(string apikey,string apisecret)
		{
			string Request = "";
			Request += addParameter("apiKey", apikey);
			Request += addParameter("count","2000");
			Request += addParameter("from","1");
			Request += addParameter("handle","ZANATY_");
			Request += addParameter("time", new Converters().generateTimeInUnix().ToString());
			Request = Request.Substring(0, Request.Length - 1);
			var ApiSig = generateSig(Request, "/user.status?",apisecret);
			if (ApiSig == null) return null;
			Request += "&";
			Request += addParameter("apiSig", ApiSig);
			Request = Request.Substring(0, Request.Length - 1);
			return Request;
		}
		private string generateContestStatusRequest(string contestid,string handle, string apikey,string apisecret)
		{
			string Parameters = "";
			Parameters += addParameter("apiKey", apikey);
			Parameters += addParameter("asManager", "false");
			Parameters += addParameter("contestId", contestid);
			Parameters += addParameter("count", "5");
			Parameters += addParameter("from", "1");
			if(handle.Length>0)
			Parameters += addParameter("handle", handle);
			Parameters += addParameter("time",new Converters().generateTimeInUnix().ToString());
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			var ApiSig = generateSig(Parameters, "/contest.status?",apisecret);
			if (ApiSig == null) return null;
			Parameters += "&";
			Parameters += addParameter("apiSig", ApiSig);
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			return Parameters;
		}
		private string generateSig(string parameters,string controller,string apisecret)
		{
			string Rand = new Random().Next(100000, 999999).ToString();
			string Link = Rand + controller + parameters;
			Link += "#" + apisecret;
			try
			{
				SHA512 sha512 = SHA512.Create();
				byte[] hashValue = sha512.ComputeHash(Encoding.UTF8.GetBytes(Link));
				string HashText = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
				while (HashText.Length < 32)
				{
					HashText = "0" + HashText;
				}
				return Rand+HashText;
			}
			catch
			{
				return null;
			}
		}
		private string addParameter(string key, string val) {
			string Parameter = "";
			Parameter += key;
			Parameter += "=";
			Parameter += val;
			Parameter+= "&";
			return Parameter;
		}
	}
}