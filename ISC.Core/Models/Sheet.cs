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
		public int NumberOfProblems { get; set; } = 0;
	}
}
