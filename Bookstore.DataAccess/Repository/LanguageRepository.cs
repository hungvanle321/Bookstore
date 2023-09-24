using Bookstore.DataAccess.Data;
using Bookstore.Models;
using Bookstore.DataAccess.Repository.IRepository;

namespace Bookstore.DataAccess.Repository
{
	public class LanguageRepository : Repository<Language>, ILanguageRepository
	{
		private readonly ApplicationDbContext _context;
		public LanguageRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public void Update(Language language)
		{
			_context.Languages.Update(language);
		}
	}
}
