using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Session
	{
		public int Id { get; set; }
		public string Topic { get; set; }
		public string InstructorName { get; set; }
		public DateTime Date { get; set; }
		public string? LocationName { get; set; }
		public string? LocationLink { get; set; }
		public int CampId { get; set; }
	}
}
