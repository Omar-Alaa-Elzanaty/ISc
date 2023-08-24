using System.Net.Http.Headers;
using System.Text.Json;
using static ISC.API.APIDtos.CodeForcesDtos;

namespace ISC.API.Services
{
	public class ApiRequestServices
	{
		private readonly string _BaseLink;
        private readonly HttpClient _HttpClient;
        public ApiRequestServices(string baselink)
        {
            _HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(baselink)
            };
			_HttpClient.DefaultRequestHeaders
					.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_BaseLink = baselink;
        }
        public async Task<object> getRequestAsync<T>(string Parameter)
        {
            HttpResponseMessage Response=await _HttpClient.GetAsync(_BaseLink+Parameter);
            if(Response.IsSuccessStatusCode!=true)
            {
                return null; 
            }
			var ResponseContent = await Response.Content.ReadAsAsync<T>();
            return ResponseContent;
		}
    }
}
