using ISC.API.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ISC.API.Services
{
	public class MailServices : IMailServices
	{
		private readonly MailSettings _MailSettings;
        public MailServices(IOptions<MailSettings> mailsettings)
        {
            _MailSettings = mailsettings.Value;
        }
		public async Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
		{
			var email = new MimeMessage()
			{
				Sender = MailboxAddress.Parse(_MailSettings.Email),
				Subject = subject,
			};

			email.To.Add(MailboxAddress.Parse(mailTo));

			var builder = new BodyBuilder();

			if (attachments != null)
			{
				byte[] fileBytes;
				foreach (var file in attachments)
				{
					if (file.Length > 0)
					{
						using var ms = new MemoryStream();
						file.CopyTo(ms);
						fileBytes = ms.ToArray();

						builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
					}
				}
			}

			builder.HtmlBody = body;
			email.Body = builder.ToMessageBody();
			email.From.Add(new MailboxAddress(_MailSettings.DisplayName, _MailSettings.Email));

			using var smtp = new SmtpClient();
			smtp.Connect(_MailSettings.Host, _MailSettings.Port, SecureSocketOptions.StartTls);
			smtp.Authenticate(_MailSettings.Email, _MailSettings.Password);
			await smtp.SendAsync(email);

			smtp.Disconnect(true);
		}
	}
}
