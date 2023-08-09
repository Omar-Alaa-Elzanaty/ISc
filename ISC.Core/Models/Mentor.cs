using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Mentor
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public virtual List<Trainee> Trainees { get; set; }
		public virtual HashSet<MentorOfCamp> Camps { get; set; }
		public virtual HashSet<MentorAttendence> Attendence { get; set; }
	}
}
