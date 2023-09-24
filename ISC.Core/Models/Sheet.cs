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
		public string SheetCfId { get;set; }
		public bool IsSohag { get; set; }
		public int SheetOrder { get; set; }
		public int MinimumPrecent { get; set; }
		public virtual List<Material> Materials { get; set; }
		public int CampId { get; set; }
		public virtual Camp Camp { get; set; }
		public virtual HashSet<TraineeSheetAccess> TraineesAccessing { get; set; }
	}
}
