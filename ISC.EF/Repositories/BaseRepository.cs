using ISC.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class BaseRepository<T> : IBaseRepository<T> where T : class
	{
		private readonly DataBase _Context;
        public BaseRepository(DataBase context)
		{
			_Context = context;
		}
		public async Task<T> getByIdAsync(int id)
		{
			return await _Context.Set<T>().FindAsync(id);
		}
		public IQueryable<T> Get()
		{
			return  _Context.Set<T>();
		}
		public async Task<T> findByAsync(Expression<Func<T, bool>> match)
		{
			return await _Context.Set<T>().SingleOrDefaultAsync(match);
		}
		public async Task Update(T entity)
		{
			_Context.Set<T>().Update(entity);
		}
		public async Task<List<T>> findManyWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null)
		{
			IQueryable<T> query = _Context.Set<T>();
			if (includes != null)
			{
				foreach (var item in includes)
				{
					query = query.Include(item);
				}
			}
			return await query.Where(match).ToListAsync();
		}
		public async Task<T?> findWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null)
		{
			IQueryable<T> query = _Context.Set<T>();
			if (includes != null)
			{
				foreach (var item in includes)
				{
					query = query.Include(item);
				}
			}
			return await query.SingleOrDefaultAsync(match)??null;
		}
		public async void addAsync(T entity)
		{ 
			await _Context.Set<T>().AddAsync(entity);
		}
		private async Task<bool> deleteEntityAsync(T entity)
		{
			try
			{
				_Context.Set<T>().Remove(entity);
				return true;
			}
			catch
			{
				return false;
			}
		}
		public void deleteGroup(List<T> entities)
		{
			_Context.Set<T>().RemoveRange(entities);
		}
		public async Task<List<T>> getAllAsync()
		{
			return await _Context.Set<T>().ToListAsync();
		}
		public async Task<List<T>> getAllwithNavigationsAsync(string[] includes = null)
		{
			IQueryable<T> Query = _Context.Set<T>();
			foreach(var include in includes)
			{
				Query = Query.Include(include);
			}
			return await Query.ToListAsync();
		}
		public virtual Task<bool> deleteAsync(T entity)
		{
			return deleteEntityAsync(entity);
		}

		public async Task<List<T>> getAllAsync(Expression<Func<T, bool>> match)
		{
			return await _Context.Set<T>().Where(match).ToListAsync();
		}
	}
}
