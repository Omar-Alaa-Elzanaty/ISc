using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class DisplayCampsDto
	{
		public DisplayCampsDto()
		{
            CampMentors = new List<Member>();
			HeadsInfo = new List<Member>();
		}
        public int Id { get; set; }
		public string Name { get; set; }
		public int Term { get; set; }
		public int Year { get; set; }
		public int DurationInWeeks { get; set; }
		public List<Member> HeadsInfo { get; set; }
		public List<Member> CampMentors { get; set; }
	}
	public class Member
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool State { get; set; }
	}
}
