using ISC.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface IBaseRepository<T>where T : class
	{
		Task AddAsync(T entity);
		Task AddGroup(List<T> group);
		Task<T> GetByIdAsync(int id);
		IQueryable<T> Get();
		Task UpdateAsync(T entity);
        Task<List<T>> GetAllAsync();
		Task<List<T>>GetAllAsync(Func<T, bool> match);
		void RemoveGroup(List<T> entities);
		Task<bool> DeleteAsync(T entity);
		int DeleteAll();
		Task<List<T>> GetAllwithNavigationsAsync(string[] includes = null);
		Task<List<T>> FindManyWithChildAsync(Func<T, bool> match, string[] includes = null);
		Task<T?> FindWithChildAsync(Func<T, bool> match, string[] includes = null);
		Task<T> FindByAsync(Func<T, bool> match);
		Task<List<T>> FindWithMany(string[] includes = null);
	}
}
