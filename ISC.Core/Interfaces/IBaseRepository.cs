using ISC.Core.Models;
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
		Task<bool> delete(T entity);
		Task<T> getByIdAsync(int id);
		T getById(int id);
		Task<List<T>> getAllAsync();
		List<T> getAll();
		void deleteGroup(List<T> entities);
		Task<bool> deleteEntityAsync(T entity);
		Task<List<T>> getAllwithNavigationsAsync(string[] includes = null);
		Task<List<T>> findManyWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null);
		Task<T> findWithChildAsync(Expression<Func<T, bool>> match, string[] includes = null);
		//Task<T> findAsync(Expression<Func<T, bool>> match);
	}
}
