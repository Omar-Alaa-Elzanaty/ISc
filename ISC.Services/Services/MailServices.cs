using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ISC.Services.Services
{
    public class MailServices : IMailServices
	{
		private readonly MailSettings _MailSettings;
		private readonly IAccountRepository _AccountRepository;
        public MailServices(IOptions<MailSettings> mailsettings,IUnitOfWork unitofwork,IAccountRepository accountRepository)
        {
            _MailSettings = mailsettings.Value;
			_AccountRepository = accountRepository;
        }
		public async Task<bool> sendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
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
			try
			{
				smtp.Connect(_MailSettings.Host, _MailSettings.Port, SecureSocketOptions.StartTls);
				smtp.Authenticate(_MailSettings.Email, _MailSettings.Password);
				await smtp.SendAsync(email);
			}
			catch
			{
				smtp.Disconnect(true);
				return false;
			}

			smtp.Disconnect(true);
			return true;
		}
		/// <summary>
		///I think that is function may be implement in endpoint
		/// </summary>
		public async Task<List<NewRegistration>> sendMailToAcceptedAsync(List<NewRegistration>trainees,string subject,string body)
		{
			List<NewRegistration>NotValidAccount = new List<NewRegistration>();
			foreach(var Trainee in trainees)
			{
				//logic of html body (method should impelement in converter)
				UserAccount NewUser = new UserAccount();
				string password = string.Empty;
				if(await sendEmailAsync(Trainee.Email, subject, body))
				{
					if (await _AccountRepository.tryCreateTraineeAccountAsync(NewUser, password) == false)
					{
						NotValidAccount.Add(Trainee);
						await sendEmailAsync(Trainee.Email, "Account Problem", "Please Contact with Us!"); 
					}
				}
				else
				{
					NotValidAccount.Add(Trainee);
				}
				
			}
			if (NotValidAccount.Count == 0)
				return null;

			return NotValidAccount;
		}
	}
}
