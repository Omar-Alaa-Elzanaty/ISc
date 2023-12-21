namespace ISC.Core.APIDtos
{
	public class StuffNewRolesDto
	{
		public List<string> UsersIds { get; set; }
		public RoleDto UserRole { get; set; }
	}
	public class RoleDto
	{
		public string Role { get; set;}
		public int? CampId { get; set; }
	}
}
