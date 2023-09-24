using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;

namespace MyWeb.DataAccess.Repository.IRepository
{
	public interface IBookRepository : IRepository<Book>
	{
		void Update(Book book);
	}
}