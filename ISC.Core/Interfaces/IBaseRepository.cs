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
		Task addAsync(T entity);
		Task AddGroup(List<T> group);
		Task<T> getByIdAsync(int id);
		IQueryable<T> Get();
		Task UpdateAsync(T entity);
        Task<List<T>> getAllAsync();
		Task<List<T>>getAllAsync(Func<T, bool> match);
		void deleteGroup(List<T> entities);
		Task<bool> deleteAsync(T entity);
		int DeleteAll();
		Task<List<T>> getAllwithNavigationsAsync(string[] includes = null);
		Task<List<T>> findManyWithChildAsync(Func<T, bool> match, string[] includes = null);
		Task<T?> findWithChildAsync(Func<T, bool> match, string[] includes = null);
		Task<T> findByAsync(Func<T, bool> match);
		Task<List<T>> FindWithMany(string[] includes = null);
	}
}
