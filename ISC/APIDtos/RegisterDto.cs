using Microsoft.AspNetCore.Http.Metadata;
using System.ComponentModel.DataAnnotations;

namespace ISC.API.APIDtos
{
	public class RegisterDto
	{
		[MaxLength(10)]
		public string FirstName { get; set; }
		[MaxLength(10)]
		public string MiddleName { get; set; }
		[MaxLength(10)]
		public string LastName { get; set; }
		[RegularExpression("^[1-9][0-9]*$")]
		public string NationalId { get; set; }
		public string Email{ get; set; }
		public string BirthDate { get; set; }
		public int Grade { get; set; }
		[MaxLength(30)]
		public string College { get; set; }
		public string Gender { get; set; }
		public IFormFile? ProfilePicture { get; set; }
		public string? PhoneNumber { get; set; }
		[MaxLength(25)]
		public string CodeForceHandle { get; set; }
		public string? FacebookLink { get; set; }
		public string? VjudgeHandle { get; set; }
		public List<string> Roles { get; set; }
		public int? MentorId { get; set; }
		public int? CampId { get; set; }
	}
}
