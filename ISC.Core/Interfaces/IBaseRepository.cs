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
		Task<List<T>> getAllwithNavigationsAsync(string[] includes = null);
		Task<string> testvirtual();
		//Task<T> findAsync(Expression<Func<T, bool>> match);
		//Task<T> findWithChildAsync(Expression<Func<T, bool>> match, string[]includes=null);
	}
}
