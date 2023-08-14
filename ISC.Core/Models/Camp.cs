using ISC.Core.ModelsDtos;
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
		public List<Trainee> Trainees { get; set; }
		public HashSet<MentorOfCamp> MentorsOfCamp { get; set; }
		public List<Session> Sessions { get; set; }
		public List<HeadOfTraining> Heads { get; set; }

	}
}
