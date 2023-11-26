using ISC.Core.Dtos;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface ISessionRepository : IBaseRepository<Session>
	{
		Task<ServiceResponse<bool>> CheckUpdateAbility(Session oldsession, SessionDto newSession,int id);
	}
}
