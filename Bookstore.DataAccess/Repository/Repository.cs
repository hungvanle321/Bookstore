using System.Linq.Expressions;
using Bookstore.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Bookstore.DataAccess.Repository.IRepository;

namespace MyWeb.DataAccess.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;
		internal DbSet<T> dbSet;
		public Repository(ApplicationDbContext db) { 
			_context = db;
			dbSet = _context.Set<T>();
		}
		public async Task AddAsync(T entity)
		{
			await dbSet.AddAsync(entity);
		}

		// Use IncludeProperties to join multiple tables
		public async Task<T> GetAsync(Expression<Func<T, bool>> filter, string? IncludeProperties = null, bool tracked = false)
		{
			IQueryable<T> query;

            if (tracked)
			{
                query = dbSet;
                
            }
			else
			{
				query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (!string.IsNullOrEmpty(IncludeProperties))
            {
                foreach (var property in IncludeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

		public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? IncludeProperties = null)
		{
			IQueryable<T> query = dbSet;
			if (filter!=null) 
				query = query.Where(filter);
			if (!string.IsNullOrEmpty(IncludeProperties))
			{
				foreach (var property in IncludeProperties
					.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(property);
				}
			}
			return await query.ToListAsync();
		}

		public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entities)
		{
			dbSet.RemoveRange(entities);
		}
	}
}
