using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class TraineeInfoDto
	{
		public string FullName { get; set;} 
		public List<bool> IsAttend { get; set;}=new List<bool>();
	}
	public class TraineeAttendenceDto
	{
		public List<string>Sessions { get; set; }=new List<string>();
		public List<TraineeInfoDto> Attendances { get; set; }= new List<TraineeInfoDto>();
	}
}
