using ISC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Trainee
	{
		public int Id { get; set; }
		public int TotalSolvedProblems { get; set; } = 0;
		public string UserId { get; set; }
		public int MentorId { get; set; }
		public virtual Mentor Mentor { get; set;}

	}
}
