using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Models.ViewModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Routing;

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

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Books(string? searchString, string? sortType, List<string> categories, List<string> authors, List<string> publishers, int? pageNumber, string? lower_price, string? upper_price)
		{
			IEnumerable<Book> books = await _unitOfWork.BookRepo.GetAllAsync(IncludeProperties: "Category");

			if (!string.IsNullOrEmpty(searchString))
			{
				books = books.Where(p => p.Title.ToUpper().Contains(searchString.ToUpper()));
			}
			var initialBooks = books;

			var isFirstSearched = (authors.Count == 0 && categories.Count == 0 && publishers.Count == 0);
			if (!isFirstSearched)
			{
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

				if (publishers.Count > 0)
					books = books.Where(p=> publishers.Contains(p.Publisher));
			}

			bool hasBooks = books.Any();
            
            double low = -1;
			double high = -1;

			if (lower_price != null)
			{
				low = double.Parse(lower_price);
				if (hasBooks)
				{
					books = books.Where(p => p.DiscountPrice >= low);
					isFirstSearched = false;
				}
					
			}

			if (upper_price != null)
			{
				high = double.Parse(upper_price);
				if (hasBooks)
				{
					books = books.Where(p => p.DiscountPrice <= high);
					isFirstSearched = false;
				}
			}

			var booksForFrequency = isFirstSearched ? books : initialBooks;

			var authorFrequency = booksForFrequency.GroupBy(p => p.Author).Select(g => new { Author = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).Take(30);
			var categoryFrequency = booksForFrequency.GroupBy(p => p.Category.CategoryName).Select(g => new { Category = g.Key, Count = g.Count()}).OrderByDescending(g => g.Count).Take(30);
			var publisherFrequency = booksForFrequency.GroupBy(p => p.Publisher).Select(g => new { Publisher = g.Key, Count = g.Count()}).OrderByDescending(g => g.Count).Take(30);

			int pageSize = 8;
			if (!hasBooks)
				books = Enumerable.Empty<Book>();

			BookHomePageViewModel bookHomePageViewModel = new()
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
					IsChecked = categories.Contains(c.Category),
					Name = c.Category,
					Count = c.Count
				}),
				PublisherList = publisherFrequency.Select(p => new CheckBoxOption()
				{
					IsChecked = publishers.Contains(p.Publisher),
					Name = p.Publisher,
					Count = p.Count
				}),
				SearchString = searchString,
				SortType = (string.IsNullOrEmpty(sortType)) ? "" : sortType,
				MinPrice = low != -1 ? low : null,
				MaxPrice = high != -1 ? high : null
			};


			return View(bookHomePageViewModel);
		}

		[HttpPost]
		public async Task<IActionResult> AddToCart(int bookId)
		{
			if (!User.Identity.IsAuthenticated)
			{
				return Json(new { success = false, url = "/Identity/Account/Login" });
			}

			var claimedIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var shoppingCart = new ShoppingCart()
			{
				ApplicationUserId = userId,
				BookId = bookId,
				Count = 1
			};
			var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(s => s.ApplicationUserId == shoppingCart.ApplicationUserId && s.BookId == shoppingCart.BookId);
			if (cartFromDb == null)
			{
				await _unitOfWork.ShoppingCartRepo.AddAsync(shoppingCart);
				await _unitOfWork.SaveAsync();
				HttpContext.Session.SetInt32(StaticDetails.SessionCart,
					(await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == shoppingCart.ApplicationUserId)).Count());
				return Json(new { success = true, message = "Book added to cart successfully!" });
			}
			else
			{
				cartFromDb.Count += shoppingCart.Count;
				_unitOfWork.ShoppingCartRepo.Update(cartFromDb);
				await _unitOfWork.SaveAsync();
				return Json(new { success = true, message = "Book counts updated successfully!" });
			}
			
		}

		public async Task<IActionResult> Details(int id)
		{
			ShoppingCart shoppingCart = new()
			{
				Book = await _unitOfWork.BookRepo.GetAsync(p => p.BookId == id, IncludeProperties: "Category,Language"),
				Count = 1,
				BookId = id
			};

			return View(shoppingCart);
		}

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Details(ShoppingCart shoppingCart)
		{
			if (!User.Identity.IsAuthenticated)
			{
				return RedirectToPage("/Identity/Account/Login");
			}

			var claimedIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			shoppingCart.ApplicationUserId = userId;
			var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(s => s.ApplicationUserId == shoppingCart.ApplicationUserId && s.BookId == shoppingCart.BookId);
			if (cartFromDb == null)
			{
				await _unitOfWork.ShoppingCartRepo.AddAsync(shoppingCart);
				await _unitOfWork.SaveAsync();
				HttpContext.Session.SetInt32(StaticDetails.SessionCart,
					(await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == shoppingCart.ApplicationUserId)).Count());
				TempData["success"] = "Book added to cart successfully!";
			}
			else
			{
				cartFromDb.Count += shoppingCart.Count;
				_unitOfWork.ShoppingCartRepo.Update(cartFromDb);
				await _unitOfWork.SaveAsync();
				TempData["success"] = "Book counts updated successfully!";
			}

			return RedirectToAction(nameof(Books));
		}

		[HttpGet]
		public IActionResult GetViewComponent()
		{
			return ViewComponent("Cart");
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