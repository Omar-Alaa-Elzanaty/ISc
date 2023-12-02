using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class SheetInfo
	{
		public int Id { get; set;}
		public string Name { get; set;}
		public int Count { get; set; }
		public int Total { get; set; }
	}
	public class TraineeStanding
	{
		public string Name { get; set; }
		public List<SheetInfo> stand { get; set; }

		public TraineeStanding()
		{
			Name = " ";
			stand = new List<SheetInfo>();
		}
	}
	public class GeneralStandingDto
	{
		public GeneralStandingDto()
		{
			Standing = new List<TraineeStanding>();
			CampName = "";
		}
		public string CampName { get; set; }
		public List<TraineeStanding> Standing { get; set; }

	}
}
