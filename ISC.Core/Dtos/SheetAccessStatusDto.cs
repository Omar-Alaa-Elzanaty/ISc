using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class SheetAccessStatusDto
	{
		public int SheetId { get; set; }
		public string Name { get; set; }
		public bool IsReachAble { get; set; }
	}
}
