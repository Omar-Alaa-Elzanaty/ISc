using ISC.Core.Models;

namespace ISC.API.ISerivces
{
    public interface IMailServices
    {
        Task <bool>sendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);
        Task<List<NewRegitseration>> sendMailToAcceptedAsync(List<NewRegitseration> trainees, string subject, string body);

	}
}
