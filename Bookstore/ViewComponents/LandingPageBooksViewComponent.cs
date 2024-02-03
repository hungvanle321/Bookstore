using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.ViewComponents
{
	public class LandingPageBooksViewModel
	{
		public List<Book> BookList { get; set; }
		public string Criteria { get; set; }
	}
	public class LandingPageBooksViewComponent : ViewComponent
	{
		private readonly IUnitOfWork _unitOfWork;
		public LandingPageBooksViewComponent(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IViewComponentResult> InvokeAsync(string criteria)
		{
			LandingPageBooksViewModel landingPageBooksViewModel = new() { Criteria = criteria };
			landingPageBooksViewModel.BookList = criteria switch
			{
				StaticDetails.LandingPage_Newest => (await _unitOfWork.BookRepo.GetAllAsync()).OrderByDescending(b => b.CreatedAt).Take(StaticDetails.RelatedBooksCount).ToList(),
				StaticDetails.LandingPage_Manga => (await _unitOfWork.BookRepo.GetAllAsync(b => b.Category.CategoryName == "Manga")).Take(StaticDetails.RelatedBooksCount).ToList(),
				StaticDetails.LandingPage_Fantasy => (await _unitOfWork.BookRepo.GetAllAsync(b => b.Category.CategoryName == "Fantasy")).Take(StaticDetails.RelatedBooksCount).ToList(),
				StaticDetails.LandingPage_ScienceFiction => (await _unitOfWork.BookRepo.GetAllAsync(b => b.Category.CategoryName == "Science Fiction")).Take(StaticDetails.RelatedBooksCount).ToList(),
				StaticDetails.LandingPage_Romance => (await _unitOfWork.BookRepo.GetAllAsync(b => b.Category.CategoryName == "Romance")).Take(StaticDetails.RelatedBooksCount).ToList(),
				_ => new List<Book>(),
			};
			return View(landingPageBooksViewModel);
		}
	}
}
