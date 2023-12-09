using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class PersonInfoDto
	{
		public string FullName { get; set;} 
		public List<bool> IsAttend { get; set;}=new List<bool>();
	}
	public class PersonAttendenceDto
	{
        public PersonAttendenceDto()
        {
			Attendances = new List<PersonInfoDto>();
			Sessions = new List<string>();
        }

        public List<string>Sessions { get; set; }
		public List<PersonInfoDto> Attendances { get; set; }
	}
}
