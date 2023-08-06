using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF
{
	public class UserAccount:IdentityUser
	{
		[Required,MaxLength(20)]
		public string FirstName { get; set; } = "NOName";
		[Required,MaxLength(20)]
		public string MiddleName { get; set; }
		[Required, MaxLength(20)]
		public string LastName { get; set; }
		[Required]
		public int NationalId { get; set; }
		[Required]
		public DateTime BirthDate { get; set; }
		[Required]
		public int Grade { get; set; }
		[Required, MaxLength(20)]
		public string College { get; set; }
		public DateTime JoinDate { get; set; }= DateTime.Now;
		[Required, MaxLength(20)]
		public string Gender { get; set; }
		public DateTime LastLoginDate { get; set; }=DateTime.Now;
		public byte[]? ProfilePicture { get; set; }
		[Required]
		public string CodeForceHandle { get; set; }
		public string? FacebookLink { get; set; }
		public string? VjudgeHandle { get; set; }
	}
}
