using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class TraineeMentorDto
	{
		public int TraineeId { get; set; }
		public string TraineeName { get; set; }
		public int? MentorId { get; set; } = null;
		public string? MentorName { get; set; } = null;
	}
}
