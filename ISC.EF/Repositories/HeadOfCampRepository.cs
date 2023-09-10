using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class HeadOfCampRepository:BaseRepository<HeadOfTraining>,IHeadOfCampRepository
	{
		private readonly DataBase _Context;
		public HeadOfCampRepository(DataBase context) : base(context)
		{
			_Context = context;
		}

		public async Task<bool> deleteEntityAsync(string userid)
		{
			var head=await _Context.HeadsOfTraining.Where(head=>head.UserId == userid).FirstOrDefaultAsync();
			if (head == null) return false;

			return await delete(head);
		}
	}
}
