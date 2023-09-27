﻿using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repository.IRepository;

namespace Bookstore.DataAccess.Repository
{
	public class UnitOfWork : IUnitOfWork
    {
		public IBookRepository BookRepo { get; private set; }
		public ILanguageRepository LanguageRepo { get; private set; }
		public ICategoryRepository CategoryRepo { get; private set; }
		public IShoppingCartRepository ShoppingCartRepo { get; private set; }

		private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
			BookRepo = new BookRepository(_context);
			LanguageRepo = new LanguageRepository(_context);
			CategoryRepo = new CategoryRepository(_context);
			ShoppingCartRepo = new ShoppingCartRepository(_context);
		}
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
