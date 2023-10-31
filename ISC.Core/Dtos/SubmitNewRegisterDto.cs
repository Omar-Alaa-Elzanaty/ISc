using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class ContestInfo
	{
		public int ContestId { get; set; }
		public float PassingPrecent { get; set; }
		public bool IsSohag { get; set; }
	}
	public class SubmitNewRegisterDto
	{
		public List<ContestInfo> ContestsInfo { get; set; }
		public int CampId { get; set; }
		public List<string>CandidatesNationalId { get; set; }
	}

}
