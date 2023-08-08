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
		public virtual HashSet<Trainee> Trainees { get; set; }
	}
}
