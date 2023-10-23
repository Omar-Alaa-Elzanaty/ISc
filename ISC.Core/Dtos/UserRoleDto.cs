using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class UserRoleDto
	{
		public List<string> Users { get; set; }
		public string? Role { get; set; }
		public int? CampId { get; set; }
		public int? MentorId { get; set; }
	}
}
