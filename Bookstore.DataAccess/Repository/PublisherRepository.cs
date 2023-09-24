using Bookstore.DataAccess.Data;
using Bookstore.Models;
using MyWeb.DataAccess.Repository.IRepository;

namespace MyWeb.DataAccess.Repository
{
	public class PublisherRepository : Repository<Publisher>, IPublisherRepository
	{
		private readonly ApplicationDbContext _context;
		public PublisherRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public void Update(Publisher publisher)
		{
			_context.Publishers.Update(publisher);
		}
	}
}
