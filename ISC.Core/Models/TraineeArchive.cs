using ISC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Models
{
	public class TraineeArchive:Archive
	{
		public string CampName { get; set; }
		public bool? IsCompleted { get; set; }
	}
}
