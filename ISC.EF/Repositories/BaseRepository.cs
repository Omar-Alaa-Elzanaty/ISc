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
		public async Task<T> findWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null)
		{
			IQueryable<T> query = _Context.Set<T>();
			if (includes != null)
			{
				foreach (var item in includes)
				{
					query = query.Include(item);
				}
			}
			return await query?.SingleOrDefaultAsync(match)??null;
		}

		public async void addAsync(T entity)
		{ 
			await _Context.Set<T>().AddAsync(entity);
		}

		public async Task<bool> delete(T entity)
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
		public List<T> getAll()
		{
			return _Context.Set<T>().ToList();
		}

		public T getById(int id)
		{
			return  _Context.Set<T>().Find(id);
		}

		public virtual Task<bool> deleteEntityAsync(T entity)
		{
			return delete(entity);
		}
	}
}
