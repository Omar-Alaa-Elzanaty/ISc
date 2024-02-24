using ISC.Core.Interfaces;
using ISC.Core.Models;
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
		public async Task<T> GetByIdAsync(int id)
		{
			return await _Context.Set<T>().FindAsync(id);
		}
		public async Task AddGroup(List<T>group)
		{
			await _Context.Set<T>().AddRangeAsync(group);
		}
		public IQueryable<T> Get()
		{
			return  _Context.Set<T>();
		}
		public async Task<T> FindByAsync(Func<T, bool> match)
		{
			return  _Context.Set<T>().SingleOrDefault(match);
		}
		public async Task UpdateAsync(T entity)
		{
			_Context.Set<T>().Update(entity);
		}
		public async Task<List<T>> FindManyWithChildAsync(Func<T, bool> match, string[] includes = null)
		{
			IQueryable<T> query = _Context.Set<T>();
			if (includes != null)
			{
				foreach (var item in includes)
				{
					query = query.Include(item);
				}
			}
			return  query.Where(match).ToList();
		}
		public async Task<T?> FindWithChildAsync(Func<T, bool> match, string[] includes = null)
		{
			IQueryable<T> query = _Context.Set<T>();
			if (includes != null)
			{
				foreach (var item in includes)
				{
					query = query.Include(item);
				}
			}
			return  query.SingleOrDefault(match)??null;
		}
		public async Task<List<T>>FindWithMany(string[] includes = null)
		{
			IQueryable<T> query = _Context.Set<T>();
			if (includes != null)
			{
				foreach (var item in includes)
				{
					query = query.Include(item);
				}
			}
			return query.ToList();
		}
		public async Task AddAsync(T entity)
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
		public void RemoveGroup(List<T> entities)
		{
			_Context.Set<T>().RemoveRange(entities);
		}
		public  int DeleteAll()
		{
			return _Context.Set<T>().ExecuteDelete();
		}
		public async Task<List<T>> GetAllAsync()
		{
			return await _Context.Set<T>().ToListAsync();
		}
		public async Task<List<T>> GetAllwithNavigationsAsync(string[] includes = null)
		{
			IQueryable<T> Query = _Context.Set<T>();
			foreach(var include in includes)
			{
				Query = Query.Include(include);
			}
			return await Query.ToListAsync();
		}
		public virtual Task<bool> DeleteAsync(T entity)
		{
			return deleteEntityAsync(entity);
		}

		public async Task<List<T>> GetAllAsync(Func<T, bool> match)
		{
			return  _Context.Set<T>().Where(match).ToList();
		}
	}
}
