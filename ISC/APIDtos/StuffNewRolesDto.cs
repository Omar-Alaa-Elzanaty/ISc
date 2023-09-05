namespace ISC.API.APIDtos
{
	public class StuffNewRolesDto
	{
		public string Id { get; set; }
		public List<string> Roles { get; set; }
		public int? MentorId { get; set; }
		public int CampId { get; set; }
	}
}
