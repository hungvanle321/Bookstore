using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository.IRepository
{
	public interface IRepository<T> where T : class
	{
		Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? IncludeProperties = null);
		Task<T> GetAsync(Expression<Func<T, bool>> filter, string? IncludeProperties = null, bool tracked = false);
		Task AddAsync(T entity);
		void Remove(T entity);
		void RemoveRange(IEnumerable<T> entities);
	}
}
