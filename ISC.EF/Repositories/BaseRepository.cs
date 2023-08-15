using ISC.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class BaseRepository<T> : IBaseRepository<T> where T : class
	{
		private DataBase _Context;
		public BaseRepository(DataBase context)
		{
			_Context = context;
		}
		public async Task<T> getByIdAsync(int id)
		{
			return await _Context.Set<T>().FindAsync(id);
		}

		public async Task<T> findAsync(Expression<Func<T, bool>> match)
		{
			return await _Context.Set<T>().SingleOrDefaultAsync(match);
		}

		public async Task<T> findWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null)
		{
			IQueryable<T>query=_Context.Set<T>();
			if(includes != null)
			{
				foreach(var item in includes)
				{
					query=query.Include(item);
				}
			}
			return await query.SingleOrDefaultAsync(match);
		}

		public async void addAsync(T entity)
		{
			 await _Context.Set<T>().AddAsync(entity);
		}

		public void delete(T entity)
		{
			 _Context.Set<T>().Remove(entity);
		}
	}
}
