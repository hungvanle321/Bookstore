using MyWeb.DataAccess.Repository.IRepository;
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

        Task SaveAsync();
    }
}
