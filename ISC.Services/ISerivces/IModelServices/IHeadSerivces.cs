using ISC.Core.Dtos;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces.IModelServices
{
    public interface IHeadSerivces
    {
		Task<ServiceResponse<List<TraineeMentorDto>>> DisplayTraineeMentorAsync(string userId);
		Task SubmitTraineeMentorAsync(List<AssignTraineeMentorDto> data);

	}
}
