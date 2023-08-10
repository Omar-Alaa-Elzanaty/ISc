using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Session
	{
		public int Id { get; set; }
		[Required]
		public string Topic { get; set; }
		[Required]
		public string InstructorName { get; set; }
		[Required]
		public DateTime Date { get; set; }
		public string? LocationName { get; set; }
		public string? LocationLink { get; set; }
		public int CampId { get; set; }
		public virtual Camp Camp { get; set; }
		public virtual HashSet<MentorAttendence> MentorsAttendences { get; set; }
		public virtual HashSet<TraineeAttendence> TraineesAttendences { get;set; }
		public virtual List<SessionFeedback> Feedbacks { get; set; }
	}
}
