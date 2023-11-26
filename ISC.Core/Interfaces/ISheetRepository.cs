using ISC.Core.Dtos;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface ISheetRepository:IBaseRepository<Sheet>
	{
		Task<bool> isValidToUpdateAsync(SheetDto model, int? campId,int id);
		Task<bool> isValidToAddAsync(SheetDto model, int? campId);
	}
}
