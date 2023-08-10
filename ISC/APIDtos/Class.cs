using ISC.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace ISC.API.Dtos
{
	public class Class
	{
		[Required]
		public string FirstName { get; set; } = "NOName";
		[Required]
		public string MiddleName { get; set; }
		[Required]
		public string LastName { get; set; }
		[Required]
		public string NationalId { get; set; }
		[Required]
		public string BirthDate { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		[Required]
		public string Email { get; set; }
		public int Grade { get; set; }
		public string College { get; set; }
		public DateTime JoinDate { get; set; }
		public string Gender { get; set; }
		public DateTime LastLoginDate { get; set; } = DateTime.Now;
		public byte[]? ProfilePicture { get; set; }
		public string CodeForceHandle { get; set; }
		public string? FacebookLink { get; set; }
		public string? VjudgeHandle { get; set; } = null;
		public string phone { get; set; }
	}
}
