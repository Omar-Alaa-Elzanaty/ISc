namespace ISC.API.Helpers
{
	public class Converters
	{
		public async Task<byte[]> photoconverterasync(IFormFile file)
		{
			if (file == null) return null;
		    using var DataStream = new MemoryStream();
			await file.CopyToAsync(DataStream);
			return DataStream.ToArray();
		}
		public string generateTimeInUnix()
		{
			return (DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000).ToString();
		}
	}
}
