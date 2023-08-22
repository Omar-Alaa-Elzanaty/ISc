using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface IMentorRepository:IBaseRepository<Mentor>
	{
		Task<object> showMentorsAccountsAsync();
	}
}
