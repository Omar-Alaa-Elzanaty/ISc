using Newtonsoft.Json;
using System;
using System.IO;

namespace ISC.API.APIDtos
{
	public class CodeForcesDtos
	{
		public class CodeforcesApiResponseDto<T>
		{
			public string status { get; set; }
			public string? comment { get; set; }
			public T result { get; set; }
		}

		public class CodeforcesUserDto
		{
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
		public class CodeforceStandingResult
		{
			//[JsonProperty("contest")]
			public CodeforcesContestDto contest { get; set; }

			//[JsonProperty("problems")]
			public List<CodeforcesProblemDto> problems { get; set; }

			//[JsonProperty("rows")]
			public List<CodeforcesRankListRowDto> rows { get; set; }

			//[JsonProperty("hack")]
			//public HackResult HackResult { get; set; }
		}
		public class CodeforcesContestDto
		{
			//[JsonProperty("id")]
			public int id { get; set; }

			//[JsonProperty("name")]
			public string name { get; set; }
			public string? preparedBy { get; set; }
			public int? difficulty { get; set; }
		}
		public class CodeforcesProblemDto
		{
			//[JsonProperty("contestId")]
			public int? contestId { get; set; }

			//[JsonProperty("index")]
			public string index { get; set; }

			//[JsonProperty("name")]
			public string name { get; set; }
			//public string type { get; set; }
			//public double? points { get; set; }
			public int? rating { get; set; }
			public List<string> tags { get; set; }
		}
		public class CodeforcesRankListRowDto
		{
			//[JsonProperty("party")]
			public CodeforcePartyDto party { get; set; }

			[JsonProperty("rank")]
			public int Rank { get; set; }

			[JsonProperty("points")]
			public double Points { get; set; }

			[JsonProperty("penalty")]
			public int? Penalty { get; set; }

			[JsonProperty("successfulHackCount")]
			public int? SuccessfulHackCount { get; set; }

			[JsonProperty("unsuccessfulHackCount")]
			public int? UnsuccessfulHackCount { get; set; }

			[JsonProperty("problemResults")]
			public List<CodeforceProblemResultDto> ProblemResults { get; set; }
			public int? lastSubmissionTimeSeconds { get; set; }
		}
        public class CodeforcePartyDto
        {
			[JsonProperty("contestId")]
			public int? ContestId { get; set; }

			[JsonProperty("members")]
			public List<CodeforceMemberDto> Members { get; set; }

			[JsonProperty("participantType")]
			public string ParticipantType { get; set; }
			public int? teamId { get; set; }
			public string? teamName { get; set; }
			public bool? ghost { get;set; }
			public int? room { get; set; }
			public int? startTimeSeconds { get; set;}

		}
		public class CodeforceMemberDto 
		{
			[JsonProperty("handle")]
			public string Handle { get; set; }
			[JsonProperty("name")]
			public string Name { get; set; }
		}
		public class CodeforceProblemResultDto
		{
			[JsonProperty("points")]
			public double Points { get; set; }

			[JsonProperty("penalty")]
			public int Penalty { get; set; }

			[JsonProperty("rejectedAttemptCount")]
			public int RejectedAttemptCount { get; set; }
		}
		
	}
}
