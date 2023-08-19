namespace ISC.API.APIDtos
{
	public class CodeForcesDtos
	{
		public class CodeforcesApiResponseDto
		{
			public string status { get; set; }
			public string? comment { get; set; }
			public List<CodeforcesUserDto> result { get; set; }
		}

		public class CodeforcesUserDto
		{
			public string handle { get; set; }
			public string? firstName { get; set; }
			public string? lastName { get; set; }
			public int rating { get; set; }
			public int maxRating { get; set; }
			public string rank { get; set; }
			public string maxRank { get; set; }
			public long lastOnlineTimeSeconds { get; set; }
			public long registrationTimeSeconds { get; set; }
			public string? country { get; set; }
			public string? city { get; set; }
			public string? organization { get; set; }
			public string? email { get; set; }
			public string? vkId { get; set; }
			public string? openId { get; set; }
			public string? titlePhoto { get; set; }
		}
	}
}
