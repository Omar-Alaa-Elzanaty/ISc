namespace ISC.API.APIDtos
{
	public class CodeForcesDtos
	{
		public class CodeforcesApiResponseDto<T>
		{
			public string status { get; set; }
			public string? comment { get; set; }
			public List<T> result { get; set; }
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
		public class CodeforceContestStandingDto
		{
			public string status { get; set; }
			public string? comment { get; set; }
			public CodeforceStandingResult result { get; set; }

		}
		public class CodeforceStandingResult
		{
			public CodeforcesContestDto contest { get; set; }
			public List<CodeforcesProblemDto> problems { get; set; }
			public List<CodeforcesRankListRowDto> rows { get; set; }
		}
		public class CodeforcesContestDto
		{
			public int id { get; set; }
			public string name { get; set; }
			public string type { get; set; }
			public string phase { get; set; }
			public bool frozen { get; set; }
			public int durationSeconds { get; set; }
			public int startTimeSeconds { get; set; }
			public int relativeTimeSeconds { get; set; }
			public string? preparedBy { get; set; }
			public string? websiteUrl { get; set; }
			public string? description { get; set; }
			public int? difficulty { get; set; }
			public string? kind { get; set; }
			public string? icpcRegion { get; set; }
			public string? country { get; set; }
			public string? city { get; set; }
			public string? season { get; set; }
		}
		public class CodeforcesProblemDto
		{
			public int? contestId { get; set; }
			public string problemsetName { get; set; }
			public string index { get; set; }
			public string name { get; set; }
			public string type { get; set; }
			public double? points { get; set; }
			public int? rating { get; set; }
			public List<string> tags { get; set; }
		}
		public class CodeforcesRankListRowDto
		{
			public CodeforcePartyDto party { get; set; }
			public int rank { get; set; }
			public double points { get; set; }
			public int penalty { get; set; }
			public int successfulHackCount { get; set; }
			public int unsuccessfulHackCount { get; set; }
			public List<CodeforceProblemResultDto> problemResults { get; set; }
			public int? lastSubmissionTimeSeconds { get; set; }
		}
        public class CodeforcePartyDto
        {
            public int? contestId { get; set; }
			public List<CodeforceMemberDto> members { get; set; }
			public string participantType { get; set; }
			public int? teamId { get; set; }
			public string? teamName { get; set; }
			public bool ghost { get;set; }
			public int? room { get; set; }
			public int? startTimeSeconds { get; set;}

		}
		public class CodeforceMemberDto 
		{ 
			public string handle { get; set; }
			public string name { get; set; }
		}
		public class CodeforceProblemResultDto
		{
			public double points { get; set; }
			public int? penalty { get; set; }
			public int rejectedAttemptCount { get; set; }
			public string type { get; set; }
			public int? bestSubmissionTimeSeconds { get; set; }
		}
    }
}
