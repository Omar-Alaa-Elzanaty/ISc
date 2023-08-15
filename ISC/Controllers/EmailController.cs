using ISC.API.APIDtos;
using ISC.API.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using System.Net;
using System.Net.Mail;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmailController : ControllerBase
	{
		private readonly IMailServices _mailServices;
        public EmailController(IMailServices mailServices)
        {
            _mailServices = mailServices;
        }
        [HttpPost("send")]
		public async Task<IActionResult> sendEmail([FromForm] MailRequestDto dto)
		{
			await _mailServices.SendEmailAsync(dto.ToEmail, dto.Subject, dto.Body,dto.Files);
			return Ok();
		}

	}
}
