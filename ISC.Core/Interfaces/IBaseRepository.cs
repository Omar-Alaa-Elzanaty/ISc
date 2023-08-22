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
		void delete(T entity);
		Task<T> getByIdAsync(int id);
		Task<List<T>> getAllAsync();
		//Task<T> findAsync(Expression<Func<T, bool>> match);
		//Task<T> findWithChildAsync(Expression<Func<T, bool>> match, string[]includes=null);
	}
}
