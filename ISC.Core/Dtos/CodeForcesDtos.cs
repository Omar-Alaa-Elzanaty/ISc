using Newtonsoft.Json;
using System;
using System.IO;

namespace ISC.Core.APIDtos
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
			public string? titlePhoto { get; set; }
		}
		public class CodeforceStandingResultDto
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
			public int? contestId { get; set; }

			public string index { get; set; }
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

			//[JsonProperty("penalty")]
			//public int? Penalty { get; set; }

			//[JsonProperty("successfulHackCount")]
			//public int? SuccessfulHackCount { get; set; }

			//[JsonProperty("unsuccessfulHackCount")]
			//public int? UnsuccessfulHackCount { get; set; }

			[JsonProperty("problemResults")]
			public List<CodeforceProblemResultDto> problemResults { get; set; }
			//public int? lastSubmissionTimeSeconds { get; set; }
		}
		public class CodeforceMemberDto 
		{
			//[JsonProperty("handle")]
			public string? handle { get; set; }
			//[JsonProperty("name")]
			public string? name { get; set; }
		}
		public class CodeforceProblemResultDto
		{
			[JsonProperty("points")]
			public double Points { get; set; }

			[JsonProperty("penalty")]
			public long? Penalty { get; set; }

			[JsonProperty("rejectedAttemptCount")]
			public int RejectedAttemptCount { get; set; }
			public int? bestSubmissionTimeSeconds { get; set; }
		}
		public class CodeforceSubmisionDto
		{
			public int id { get; set; }
			public int? contestId { get; set; }
			public CodeforcePartyDto author { get; set; }
			public CodeforceProblemDto problem { get; set; }
			public string? verdict { get; set; }
			public int? creationTimeSeconds { get; set; }
		}
		public class CodeforceProblemDto
		{
			public int? contestId { get; set; }
			public string name { get; set; }

		}
		public class CodeforcePartyDto
		{
			public int? contestId { get; set; }
			public List<CodeforceMemberDto> members { get; set; }
			public string participantType { get; set; }
		}

	}
}
