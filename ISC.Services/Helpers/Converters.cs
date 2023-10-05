using Microsoft.AspNetCore.Http;

namespace ISC.Services.Helpers
{
	public class Converters
	{
		public string generateTimeInUnix()
		{
			return (DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000).ToString();
		}
	}
}
