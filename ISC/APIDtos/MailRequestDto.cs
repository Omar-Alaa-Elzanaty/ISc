namespace ISC.API.APIDtos
{
	public class MailRequestDto
	{
		public string ToEmail { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public IList<IFormFile> Files { get; set; }
	}
}
