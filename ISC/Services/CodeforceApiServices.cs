using ISC.API.Helpers;
using ISC.API.ISerivces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using static ISC.API.APIDtos.CodeForcesDtos;
using ISC.API.Services;
using System.Collections.Generic;

namespace CodeforceApiSerivces
{
	public class CodeforceApiServices:IOnlineJudgeServices
	{
		private readonly CodeForceConnection _CFConnection;
		private readonly ApiRequestServices _ApiRequest;
		public CodeforceApiServices(IOptions<CodeForceConnection>cfconnection)
		{
			_CFConnection = cfconnection.Value;
			_ApiRequest = new ApiRequestServices("https://codeforces.com/api/");
		}
		public async Task<bool> checkHandleValidationAsync(string handle)
		{
			try
			{
				var Response = await _ApiRequest
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
		public async Task<CodeforcesApiResponseDto<CodeforceStandingResult>> getContestStandingAsync(string contestid,int numberofrows,bool unofficial)
		{
			try//377686
			{

				string request = "contest.standings?"+generatecontestStandingRequest(contestid, numberofrows,unofficial);
				var Response = await _ApiRequest.getRequestAsync<CodeforcesApiResponseDto<CodeforceStandingResult>>(request);
				var Standing= (CodeforcesApiResponseDto<CodeforceStandingResult>)Response;
				if (Standing == null) return null;
				else return Standing;
			}
			catch
			{
				return null;
			}
		}
		public async Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getUserStatusAsync()
		{
			try//377686
			{

				string request = "user.status?" + generateUserStatusRequest();
				var Response = await _ApiRequest.getRequestAsync<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>(request);
				var Standing = (CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>)Response;
				if (Standing == null) return null;
				else return Standing;
			}
			catch
			{
				return null;
			}
		}
		public async Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getContestStatus(string contestid)
		{
			try//377686
			{

				string request = "contest.status?"+generateContestStatusRequest(contestid);
				var Response = await _ApiRequest.getRequestAsync<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>(request);
				var Standing = (CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>)Response;
				if (Standing == null) return null;
				else return Standing;
			}
			catch
			{
				return null;
			}
		}
		private string generatecontestStandingRequest(string contestid,int numberofrows,bool unofficial)
		{
			string Parameters = "";
			Parameters = addParameter(Parameters, "apiKey", _CFConnection.Key);
			Parameters = addParameter(Parameters, "contestId", contestid);
			Parameters = addParameter(Parameters, "count", numberofrows.ToString());
			Parameters = addParameter(Parameters, "from", "1");
			Parameters = addParameter(Parameters, "showUnofficial", unofficial==true?"True":"false");
			Parameters = addParameter(Parameters, "time", new Converters().generateTimeInUnix().ToString());
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			var ApiSig = generateSig(Parameters, "/contest.standings?");
			if (ApiSig == null) return null;
			Parameters += "&";
			Parameters = addParameter(Parameters, "apiSig", ApiSig);
			Parameters=Parameters.Substring(0,Parameters.Length - 1);
			return  Parameters;
		}
		private string generateUserStatusRequest()
		{
			string Parameters = "";
			Parameters = addParameter(Parameters, "apiKey", _CFConnection.Key);
			Parameters = addParameter(Parameters,"handle","ZANATY_");
			Parameters = addParameter(Parameters,"from","1");
			Parameters = addParameter(Parameters,"count","5");
			Parameters = addParameter(Parameters, "time", new Converters().generateTimeInUnix().ToString());
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			var ApiSig = generateSig(Parameters, "/user.status?");
			if (ApiSig == null) return null;
			Parameters += "&";
			Parameters = addParameter(Parameters, "apiSig", ApiSig);
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			return Parameters;
		}
		private string generateContestStatusRequest(string contestid)
		{
			string Parameters = "";
			Parameters = addParameter(Parameters, "apiKey", _CFConnection.Key);
			Parameters = addParameter(Parameters, "contestId", contestid);
			Parameters = addParameter(Parameters, "handle", "ZANATY_");
			Parameters = addParameter(Parameters, "asManager", "false");
			Parameters = addParameter(Parameters, "count", "5");
			Parameters = addParameter(Parameters, "from", "1");
			Parameters = addParameter(Parameters, "time",new Converters().generateTimeInUnix().ToString());
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			var ApiSig = generateSig(Parameters, "/contest.status?");
			if (ApiSig == null) return null;
			Parameters += "&";
			Parameters = addParameter(Parameters, "apiSig", ApiSig);
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			return Parameters;
		}
		private string generateSig(string parameterwithattributes,string controller)
		{
			string Rand = new Random().Next(100000, 999999).ToString();
			string Link = Rand + controller + parameterwithattributes;
			Link += "#" + _CFConnection.Secret;
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
		private string addParameter(string parameter, string key, string val) {
			parameter += key;
			parameter += "=";
			parameter += val;
			parameter+= "&";
			return parameter;
		}
	}
}