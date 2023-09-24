using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Models.ViewModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;

namespace Bookstore.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

		public async Task<IActionResult> Books(string? searchString, string? sortType, List<string> categories, List<string> authors, int? pageNumber)
		{
			IEnumerable<Book> books = await _unitOfWork.BookRepo.GetAllAsync(IncludeProperties: "Category");
			IEnumerable<Book> searchedProducts;

			if (!string.IsNullOrEmpty(searchString))
			{
				books = books.Where(p => p.Title.ToUpper().Contains(searchString.ToUpper()));
			}

			searchedProducts = books;


			if (!string.IsNullOrEmpty(sortType))
			{
				string attribute = sortType.Split('_')[0];
				bool isAscending = sortType.Split("_")[1] == "asc";
				PropertyInfo? propertyInfo = typeof(Book).GetProperty(attribute);
				if (isAscending)
					books = books.OrderBy(p => propertyInfo?.GetValue(p, null));
				else
					books = books.OrderByDescending(p => propertyInfo?.GetValue(p, null));
			}

			if (authors.Count > 0)
				books = books.Where(a => authors.Contains(a.Author));

			if (categories.Count > 0)
				books = books.Where(p => categories.Contains(p.Category.CategoryName));

			var authorFrequency = ((authors.Count == 0 && categories.Count == 0) ? books : searchedProducts)
									.GroupBy(p => p.Author)
									.Select(g => new { Author = g.Key, Count = g.Count() }).OrderBy(s => s.Author);

			var categoryFrequency = ((authors.Count == 0 && categories.Count == 0) ? books : searchedProducts)
									.GroupBy(p => p.Category.CategoryName)
									.Select(g => new { CategoryName = g.Key, Count = g.Count() }).OrderBy(s => s.CategoryName);

			int pageSize = 4;

			BookHomePageViewModel productHomePageViewModel = new()
			{
				BookList = PaginatedList<Book>.Create(books, pageNumber ?? 1, pageSize),
				AuthorList = authorFrequency.Select(a => new CheckBoxOption()
				{
					IsChecked = authors.Contains(a.Author),
					Name = a.Author,
					Count = a.Count
				}),
				CategoryList = categoryFrequency.Select(c => new CheckBoxOption()
				{
					IsChecked = categories.Contains(c.CategoryName),
					Name = c.CategoryName,
					Count = c.Count
				}),
				SearchString = searchString,
				SortType = (string.IsNullOrEmpty(sortType)) ? "" : sortType,
			};


			return View(productHomePageViewModel);
		}

		public async Task<IActionResult> Details(string id)
		{
			ShoppingCart shoppingCart = new()
			{
				Book = await _unitOfWork.BookRepo.GetAsync(p => p.BookId == id, IncludeProperties: "Category"),
				Count = 1,
				BookId = id
			};

			return View(shoppingCart);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Details(ShoppingCart shoppingCart)
		{
			var claimedIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			shoppingCart.ApplicationUserId = userId;
			var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(s => s.ApplicationUserId == userId && s.BookId == shoppingCart.BookId);
			if (cartFromDb == null)
			{
				await _unitOfWork.ShoppingCartRepo.AddAsync(shoppingCart);
				await _unitOfWork.SaveAsync();
				HttpContext.Session.SetInt32(StaticDetails.SessionCart,
					(await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == userId)).Count());
				TempData["success"] = "Item added to cart successfully!";
			}
			else
			{
				cartFromDb.Count += shoppingCart.Count;
				_unitOfWork.ShoppingCartRepo.Update(cartFromDb);
				await _unitOfWork.SaveAsync();
				TempData["success"] = "Item counts updated successfully!";
			}

			return RedirectToAction(nameof(Books));
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		public IActionResult About()
		{
			return View();
		}
		public IActionResult Contact()
		{
			return View();
		}
	}
}