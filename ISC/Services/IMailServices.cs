namespace ISC.API.Services
{
	public interface IMailServices
	{
		Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);
	}
}
