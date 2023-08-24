using ISC.API.Helpers;
using ISC.API.ISerivces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static ISC.API.APIDtos.CodeForcesDtos;

namespace CodeforceApiSerivces
{
	public class CodeforceApiServices:IOnlineJudgeServices
	{
		private readonly HttpClient _HttpClient;
		private readonly CodeForceConnection _CFConnection; 
		public CodeforceApiServices(IOptions<CodeForceConnection>cfconnection)
		{
			_HttpClient = new HttpClient
			{
				BaseAddress = new Uri("https://codeforces.com/api/")
			};
			_HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_CFConnection = cfconnection.Value;
		}

		public async Task<bool> checkHandleValidationAsync(string handle)
		{
			try
			{
				var Response = await _HttpClient.GetAsync($"user.info?handles={handle}");

				if (!Response.IsSuccessStatusCode)
				{
					return false;
				}

				var ResponseContent = await Response.Content.ReadAsStringAsync();
				var DeserializedResponse = JsonSerializer.
										Deserialize<CodeforcesApiResponseDto<CodeforcesUserDto>>(ResponseContent);

				if (DeserializedResponse.status != "OK" || DeserializedResponse.result.Count == 0)
				{
					return false;
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
		public async Task<object> getContestStandingAsync(string contestid,int numberofrows,bool unofficial)
		{
			try//377686
			{
				string BaseLink = "https://codeforces.com/api/";
				var _HttpClientConteststanding = new HttpClient() {
					BaseAddress=new Uri(BaseLink)
				};
				_HttpClientConteststanding.DefaultRequestHeaders
					.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				var x = "contest.standings?"+generatecontestStandingrequest(contestid, numberofrows,BaseLink,unofficial);
				var Response = await _HttpClientConteststanding.GetAsync(x);
				if(!Response.IsSuccessStatusCode)
				{
					return null;
				}
				var ResponseContent = await Response.Content.ReadAsStringAsync();
				var DeserializedResponse = JsonSerializer.
										Deserialize<CodeforcesApiResponseDto<CodeforceContestStandingDto>>(ResponseContent);
				if (DeserializedResponse.status != "OK")
				{
					return null;
				}
				return DeserializedResponse;
			}
			catch
			{
				return null;
			}
		}
		private string generatecontestStandingrequest(string contestid,int numberofrows,string baseaddress,bool unofficial)
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