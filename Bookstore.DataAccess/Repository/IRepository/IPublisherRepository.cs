﻿using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;

namespace Bookstore.DataAccess.Repository.IRepository
{
	public interface IPublisherRepository : IRepository<Publisher>
	{
		void Update(Publisher publisher);
	}
}