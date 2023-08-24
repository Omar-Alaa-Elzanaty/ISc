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

				string request = "contest.standings?"+generatecontestStandingrequest(contestid, numberofrows,unofficial);
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
		private string generatecontestStandingrequest(string contestid,int numberofrows,bool unofficial)
		{
			string Parameters = "";
			Parameters = addParameter(Parameters, "apiKey", _CFConnection.Key);
			Parameters = addParameter(Parameters, "contestId", contestid);
			Parameters = addParameter(Parameters, "count", numberofrows.ToString());
			Parameters = addParameter(Parameters, "from", "1");
			Parameters = addParameter(Parameters, "showUnofficial", unofficial==true?"True":"false");
			Parameters = addParameter(Parameters, "time", generateTimeInUnix().ToString());
			Parameters = Parameters.Substring(0, Parameters.Length - 1);
			var ApiSig = generateSig(Parameters);
			if (ApiSig == null) return null;
			Parameters += "&";
			Parameters = addParameter(Parameters, "apiSig", ApiSig);
			Parameters=Parameters.Substring(0,Parameters.Length - 1);
			return  Parameters;
		}
		private string generateSig(string parameterwithattributes)
		{
			string Rand = new Random().Next(100000, 999999).ToString();
			string Link = Rand + "/contest.standings?" + parameterwithattributes;
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
		private string generateTimeInUnix()
		{
			return (DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000).ToString();
		}
	}
}