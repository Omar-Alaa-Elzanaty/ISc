using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public abstract class Archive
	{
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public int NationalID { get; set; }
		public DateTime BirthDate { get; set; }
		public int Grade { get; set; }
		public string College { get; set; }
		public string Gender { get; set; }
		public string? CodeForceHandle { get; set; }
		public string? VjudgeHandle { get; set; }
		public string? FacebookLink { get; set; }
		public string? PhoneNumber { get; set; }
		public string Email { get; set; }
	}
}
