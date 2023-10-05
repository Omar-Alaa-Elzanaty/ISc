namespace ISC.Services.APIDtos
{
	public class StuffNewRolesDto
	{
		public string UserId { get; set; }
		public List<RoleDto> UserRoles { get; set; }
	}
	public class RoleDto
	{
		public string Role { get; set;}
		public int? MentorId { get; set; }
		public int? CampId { get; set; }
	}
}
