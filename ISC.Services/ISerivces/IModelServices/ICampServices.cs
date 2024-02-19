using ISC.Core.Dtos;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces.IModelServices
{
	public interface ICampServices
	{
		Task<ServiceResponse<List<DisplayCampsDto>>> DisplayCampsDetails();
		Task<ServiceResponse<bool>> UpdateHeadAsync(int id, int? campId);
		Task<ServiceResponse<bool>> UpdateMentorAsync(int id, int campId, bool isAdd);

    }
}
