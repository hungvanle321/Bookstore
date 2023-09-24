using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;

namespace MyWeb.DataAccess.Repository.IRepository
{
	public interface ILanguageRepository : IRepository<Language>
	{
		void Update(Language language);
	}
}