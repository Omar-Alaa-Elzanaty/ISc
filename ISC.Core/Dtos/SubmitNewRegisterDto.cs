using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class ContestInfo
	{
		[Required]
		public int ContestId { get; set; }
		[Required]
		public float PassingPrecent { get; set; }
		[Required]
		public bool IsSohag { get; set; }
	}
	public class SubmitNewRegisterDto
	{
		public SubmitNewRegisterDto()
		{
			ContestsInfo = new();
			CandidatesNationalId = new();
		}

		[Required]
		public List<ContestInfo> ContestsInfo { get; set; }
		[Required]
		public int CampId { get; set; }
		[Required]
		public List<string>CandidatesNationalId { get; set; }
	}

}
