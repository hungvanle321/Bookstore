using Bookstore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IBookRepository BookRepo { get; }
        ILanguageRepository LanguageRepo { get; }
        IPublisherRepository PublisherRepo { get; }
		ICategoryRepository CategoryRepo { get; }
        IShoppingCartRepository ShoppingCartRepo { get; }
		Task SaveAsync();
    }
}
