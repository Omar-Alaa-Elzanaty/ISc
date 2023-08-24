using ISC.API.Helpers;
using ISC.API.ISerivces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using static ISC.API.APIDtos.CodeForcesDtos;

namespace CodeforceApiSerivces
{
	public class CodeforceApiServices:IOnlineJudgeServices
	{
		private readonly HttpClient _HttpClient;
		private readonly CodeForceConnection _CFConnection;
		private readonly string _BaseLink; 
		public CodeforceApiServices(IOptions<CodeForceConnection>cfconnection)
		{
			_HttpClient = new HttpClient
			{
				BaseAddress = new Uri("https://codeforces.com/api/")
			};
			_HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_CFConnection = cfconnection.Value;
			_BaseLink = "https://c...content-available-to-author-only...s.com/api/contest.standings?";
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
		private string createTimeInUnix()
		{
			var Time = new DateTimeOffset(DateTime.Now);
			return  (Time.ToUnixTimeMilliseconds() / 1000).ToString();
		}
		private async Task<string>generateLink(string parameter,string secret)
		{
			return null;
		}
	}
}