using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;

namespace Bookstore.DataAccess.Repository.IRepository
{
	public interface IBookRepository : IRepository<Book>
	{
		void Update(Book book);
	}
}