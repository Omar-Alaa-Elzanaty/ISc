using System.ComponentModel.DataAnnotations;

namespace ISC.Services.APIDtos
{
	public class LoginDto
	{
		[MaxLength(40)]
		public string UserName { get; set; }
		[MaxLength(30)]
		public string Password { get; set; }
		public bool? RememberMe { get; set; }
	}
}
