using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class AttendenceDto
	{
		public int TraineeId { get; set; }
		public string FullName { get; set; }
		public bool IsAttend { get; set; }
	}
}
