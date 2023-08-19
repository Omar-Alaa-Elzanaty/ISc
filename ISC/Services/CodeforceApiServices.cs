using ISC.API.ISerivces;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using static ISC.API.APIDtos.CodeForcesDtos;

namespace CodeforcesApiClient
{
	public class CodeforceApiServices:IOnlineJudgeServices
	{
		private readonly HttpClient _httpClient;

		public CodeforceApiServices()
		{
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://codeforces.com/")
			};
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<bool> checkHandleValidationAsync(string handle)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/user.info?handles={handle}");

				if (!response.IsSuccessStatusCode)
				{
					return false;
				}

				var responseContent = await response.Content.ReadAsStringAsync();
				var deserializedResponse = JsonSerializer.Deserialize<CodeforcesApiResponseDto>(responseContent);

				if (deserializedResponse.status != "OK" || deserializedResponse.result.Count == 0)
				{
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}