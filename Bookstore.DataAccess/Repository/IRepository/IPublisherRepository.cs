using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;

namespace MyWeb.DataAccess.Repository.IRepository
{
	public interface IPublisherRepository : IRepository<Publisher>
	{
		void Update(Publisher publisher);
	}
}