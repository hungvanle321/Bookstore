using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.ViewComponents
{
	public class RelatedBooksViewComponent : ViewComponent
	{
		private readonly IUnitOfWork _unitOfWork;
		public RelatedBooksViewComponent(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IViewComponentResult> InvokeAsync(int bookId)
		{
			var book = await _unitOfWork.BookRepo.GetAsync(b => b.BookId == bookId);
			var relatedBooks = new List<Book>();

			var sameAuthorBooks = await _unitOfWork.BookRepo.GetAllAsync(b => b.Author == book.Author && b.BookId != bookId);
			if (sameAuthorBooks.Any())
			{
				relatedBooks.AddRange(sameAuthorBooks.Take(StaticDetails.RelatedBooksCount));
			}

			if (relatedBooks.Count < StaticDetails.RelatedBooksCount)
			{
				var samePublisherBooks = await _unitOfWork.BookRepo.GetAllAsync(b => b.Publisher == book.Publisher && b.BookId != bookId && !relatedBooks.Contains(b));
				relatedBooks.AddRange(samePublisherBooks.Take(StaticDetails.RelatedBooksCount - relatedBooks.Count));
			}

			if (relatedBooks.Count < StaticDetails.RelatedBooksCount)
			{
				var sameCategoryBooks = await _unitOfWork.BookRepo.GetAllAsync(b => b.CategoryID == book.CategoryID && b.BookId != bookId && !relatedBooks.Contains(b));
				relatedBooks.AddRange(sameCategoryBooks.Take(StaticDetails.RelatedBooksCount - relatedBooks.Count));
			}

			return View(relatedBooks);
		}
	}
}
