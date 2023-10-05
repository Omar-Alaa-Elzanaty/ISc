using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class HeadOfTraining
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public int? CampId { get; set; }
		public Camp? Camp { get; set; }
	}
}
