using ISC.Core.Models;
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
		public string FirstName { get; set; } = "NOName";
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public int NationalId { get; set; }
		public DateTime BirthDate { get; set; }
		public int Grade { get; set; }
		public string College { get; set; }
		public DateTime JoinDate { get; set; }= DateTime.Now;
		public string Gender { get; set; }
		public DateTime LastLoginDate { get; set; }=DateTime.Now;
		public byte[]? ProfilePicture { get; set; }
		public string CodeForceHandle { get; set; }
		public string? FacebookLink { get; set; }
		public string? VjudgeHandle { get; set; }
		public Mentor? Mentor { get; set; }
		public Trainee? Trainee { get; set; }
	}
}
