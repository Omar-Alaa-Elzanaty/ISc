using Azure;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces.IModelServices
{
	public interface ISessionsServices
	{
		Task<ServiceResponse<HashSet<int>>> SessionFilter(List<int> traineeAttend);
	}
}
