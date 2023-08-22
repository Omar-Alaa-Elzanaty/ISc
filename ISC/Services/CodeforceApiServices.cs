using ISC.API.ISerivces;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using static ISC.API.APIDtos.CodeForcesDtos;

namespace CodeforceApiSerivces
{
	public class CodeforceApiServices:IOnlineJudgeServices
	{
		private readonly HttpClient _httpClient;

		public CodeforceApiServices()
		{
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://codeforces.com/api/")
			};
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<bool> checkHandleValidationAsync(string handle)
		{
			try
			{
				var Response = await _httpClient.GetAsync($"user.info?handles={handle}");

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
	}
}