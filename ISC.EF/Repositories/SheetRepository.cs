using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class SheetRepository : BaseRepository<Sheet>, ISheetRepository
	{
        private readonly DataBase _context;
        public SheetRepository(DataBase context):base(context)
        {
            _context= context;
        }

		public async Task<bool> isValidToAddAsync(SheetDto model, int? campId)
		{
			return (campId != null)
						  && !await _context.Sheets.AnyAsync(s => (s.Name == model.Name && s.CampId == campId) || s.SheetCfId == model.SheetCfId || s.SheetLink == model.SheetLink);

		}

		public async Task<bool> isValidToUpdateAsync(SheetDto model,int? campId,int id)
        {
			return (campId != null)
				&& !await _context.Sheets.AnyAsync(s => ((s.Name == model.Name && s.CampId == campId) || s.SheetCfId == model.SheetCfId || s.SheetLink == model.SheetLink) && s.Id != id);
		}
	}
}
