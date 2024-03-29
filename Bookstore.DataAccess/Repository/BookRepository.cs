﻿using Bookstore.DataAccess.Data;
using Bookstore.Models;
using Bookstore.DataAccess.Repository.IRepository;

namespace Bookstore.DataAccess.Repository
{
	public class BookRepository : Repository<Book>, IBookRepository
	{
		private readonly ApplicationDbContext _context;
		public BookRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
			
		public void Update(Book book)
		{
			_context.Books.Update(book);
		}
	}
}
