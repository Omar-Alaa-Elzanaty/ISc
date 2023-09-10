using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface ISessionFeedbackRepository:IBaseRepository<SessionFeedback>
	{
		Task<List<SessionFeedback>> getTopRateAsync(int count);
	}
}
