using ISC.Core.Interfaces;
using ISC.Core.ModelsDtos;
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
		//public DateTime LastSubmession { get; set; }		Not Supported
		//public int points { get; set; }		Not Supported
		public string UserId { get; set; }
		public int? MentorId { get; set; }
		public virtual Mentor Mentor { get; set;}
		public int CampId { get; set; }
		public virtual Camp Camp { get; set; }
		public virtual HashSet<SessionFeedback> SessionsFeedbacks { get; set; }
		public virtual HashSet<TraineeSheetAccess> SheetsAccessing { get; set; }
		public virtual HashSet<TraineeAttendence> TraineesAttendences { get; set; }

	}
}
