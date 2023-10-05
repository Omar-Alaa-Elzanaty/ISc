﻿using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using static ISC.Core.APIDtos.CodeForcesDtos;

namespace ISC.Services.Services
{
	public class CodeforceApiService:IOnlineJudgeServices
	{
		private readonly CodeForceConnection _CFConnection;
		private readonly ApiRequestServices _ApiRequest;
		public CodeforceApiService(IOptions<CodeForceConnection>cfconnection)
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
		public async Task<CodeforcesApiResponseDto<CodeforceStandingResultDto>> getContestStandingAsync(string contestid,int numberofrows,bool unofficial,string apikey,string apisecret)
		{
			try//377686
			{

				string request = "contest.standings?"+generatecontestStandingRequest(contestid, numberofrows,unofficial,apikey,apisecret);
				var Response = await _ApiRequest.getRequestAsync<CodeforcesApiResponseDto<CodeforceStandingResultDto>>(request);
				var Standing= (CodeforcesApiResponseDto<CodeforceStandingResultDto>)Response;
				if (Standing == null) return null;
				else return Standing;
			}
			catch
			{
				return null;
			}
		}
		public async Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getUserStatusAsync(string apikey,string apisecret)
		{
			try//377686
			{

				string request = "user.status?" + generateUserStatusRequest(apikey,apisecret);
				var Response = await _ApiRequest.getRequestAsync<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>(request);
				var UserStatus = (CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>)Response;
				if (UserStatus == null) return null;
				else return UserStatus;
			}
			catch
			{
				return null;
			}
		}
		public async Task<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>> getContestStatusAsync(string contestid,string handle,string apikey,string apisecret)
		{
			try//377686
			{
				Console.WriteLine("test okay ");
				string request = "contest.status?"+generateContestStatusRequest(contestid,handle,apikey,apisecret);
				var Response = await _ApiRequest.getRequestAsync<CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>>(request);
				var ContestStatus = (CodeforcesApiResponseDto<List<CodeforceSubmisionDto>>)Response;
				if (ContestStatus == null) return null;
				else return ContestStatus;
			}
			catch
			{
				return null;
			}
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