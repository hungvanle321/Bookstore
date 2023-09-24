﻿using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repository.IRepository;
using MyWeb.DataAccess.Repository;
using MyWeb.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
		public IBookRepository BookRepo { get; private set; }
		public ILanguageRepository LanguageRepo { get; private set; }
		public IPublisherRepository PublisherRepo { get; private set; }

		private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
			BookRepo = new BookRepository(_context);
			LanguageRepo = new LanguageRepository(_context);
			PublisherRepo = new PublisherRepository(_context);
		}
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}