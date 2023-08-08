using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Camp
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Year { get; set; }
		public int Term { get; set; }
		public int DurationInWeeks { get; set; }
		public HashSet<Mentor> Mentors { get; set; }

	}
}
