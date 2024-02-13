using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class TraineeArchiveDto
	{
		public string NationalId { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string CampName { get; set; }
		public string College { get; set; }
		public string CodeforceHandle { get; set; }
		public bool IsCompleted { get; set; }
		public DateTime BirthDate { get; set; }
		public string? VjudgeHandle { get; set; }
		public string? FacebookLink { get; set; }
		public string? PhoneNumber { get; set; }
		public string Email { get; set; }
		public int Year { get; set; }
	}
}
