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
			Mentors = new List<string>();
			HeadsInfo = new List<HeadInfo>();
		}
        public int Id { get; set; }
		public string Name { get; set; }
		public int Term { get; set; }
		public int Year { get; set; }
		public int DurationInWeeks { get; set; }
		public List<HeadInfo> HeadsInfo { get; set; }
		public List<string> Mentors { get; set; }
	}
	public class HeadInfo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool State { get; set; }
	}
}
