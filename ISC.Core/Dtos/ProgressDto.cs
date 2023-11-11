using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class TraineePrgoressDto
	{
		public string FullName { get; set; }
		public List<int> solvedProblems { get; set;}=new List<int>();
	}
	public class ProgressDto
	{
		public List<string> SheetsNames { get; set; } = new List<string>();
		public Dictionary<int, TraineePrgoressDto> Trainees { get; set; } = new Dictionary<int, TraineePrgoressDto>();
	}
}
