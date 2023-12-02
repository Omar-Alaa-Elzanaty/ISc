using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class TraineeSheetAcessDto
	{
		public List<SheetsInfo> Sheets { get; set; }
		public List<TraineeAccess> TraineeAccess { get; set; }
	}
	public class TraineeAccess
	{
		public TraineeAccess()
		{
			Statues = new List<Access>();
		}
		public string FullName { get; set; }
		public ICollection<Access> Statues { get; set; }
	}
	public class Access
	{
		public int SheetId { get; set; }
		public bool HasAccess { get; set; }
	}
	public class SheetsInfo
	{
		public string Name { get; set; }
		public int Id { get; set; }
	}
}
