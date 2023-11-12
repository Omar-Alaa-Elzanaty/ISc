using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class DisplayCampsDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Term { get; set; }
		public int Year { get; set; }
		public int DurationInWeeks { get; set; }
		public List<string> Mentors { get; set; } = new List<string>();
	}
}
