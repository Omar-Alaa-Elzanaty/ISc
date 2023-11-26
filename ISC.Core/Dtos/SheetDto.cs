using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class SheetDto
	{
		public string Name { get; set; }
		public string SheetLink { get; set; }
		public string SheetCfId { get; set; }
		public bool IsSohag { get; set; }
		public int SheetOrder { get; set; }
		public int MinimumPrecent { get; set; }
	}
}
