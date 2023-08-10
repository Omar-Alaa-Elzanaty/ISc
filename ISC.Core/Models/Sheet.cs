using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Sheet
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string SheetLink { get; set; }
		public virtual List<Material> Materials { get; set; }
		public virtual HashSet<TraineeSheetAccess> TraineesAccessing { get; set; }
	}
}
