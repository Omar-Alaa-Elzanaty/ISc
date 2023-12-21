using ISC.Core.Dtos;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces
{
	public interface IPublicSerives
	{
		Task<ServiceResponse<string>> AddNewRegister(NewRegisterationDto model);
	}
}
