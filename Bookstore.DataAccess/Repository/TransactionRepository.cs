using Bookstore.DataAccess.Data;
using Bookstore.Models;
using Bookstore.DataAccess.Repository.IRepository;

namespace Bookstore.DataAccess.Repository
{
	public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
		private readonly ApplicationDbContext _context;
		public TransactionRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
