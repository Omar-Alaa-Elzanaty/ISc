using ISC.Core.Models;
using Microsoft.AspNetCore.Http;

namespace ISC.Services.ISerivces
{
    public interface IMailServices
    {
        Task <bool>sendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);
        Task<List<NewRegistration>> sendMailToAcceptedAsync(List<NewRegistration> trainees, string subject, string body);

	}
}
