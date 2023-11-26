using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class Material
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Link { get; set; }
		public DateTime CreationDate { get; set; }=DateTime.Now;
		public int SheetId { get; set; }
		public virtual Sheet Sheet { get; set; }
	}
}
