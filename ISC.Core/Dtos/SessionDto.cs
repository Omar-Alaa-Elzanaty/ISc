using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class SessionDto
	{
		public string Topic { get; set; }
		public string InstructorName { get; set; }

		public DateTime Date { get; set; }
		public string? LocationName { get; set; }
		public string? LocationLink { get; set; }
		public int CampId { get; set; }
	}
}
