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
		void addAsync(T entity);
		Task<T> getByIdAsync(int id);
		IQueryable<T> Get();
		Task Update(T entity);
		Task<List<T>> getAllAsync();
		Task<List<T>>getAllAsync(Expression<Func<T, bool>> match);
		void deleteGroup(List<T> entities);
		Task<bool> deleteAsync(T entity);
		int DeleteAll();
		Task<List<T>> getAllwithNavigationsAsync(string[] includes = null);
		Task<List<T>> findManyWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null);
		Task<T?> findWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null);
		Task<T> findByAsync(Expression<Func<T, bool>> match);
	}
}
