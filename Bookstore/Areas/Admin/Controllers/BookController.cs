using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModel;
using Bookstore.Utility;
using System.Data;

namespace MyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class BookController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BookController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = db;
            _webHostEnvironment = webHostEnvironment;
        }
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> CreateAndEdit(string? id)
        {
			BookEditViewModel bookVM = new()
			{
				CategoryList = (await _unitOfWork.CategoryRepo.GetAllAsync()).Select(
				u => new SelectListItem { Text = u.CategoryName, Value = u.CategoryId }
				),
				Book = new Book()
			};


            if (string.IsNullOrEmpty(id))
            {
                //Create
                return View(bookVM);
            }
            else
            {
				bookVM.Book = await _unitOfWork.BookRepo.GetAsync(c => c.BookId == id);
                return View(bookVM);
			}
		}

        [HttpPost]
		public async Task<IActionResult> CreateAndEdit(BookEditViewModel bookVM, IFormFile? file)
		{
            //Validation
			if (bookVM.Book.OriginPrice < bookVM.Book.DiscountPrice)
			{
				ModelState.AddModelError("Book.DiscountPrice", "The discount price cannot be greater than original.");
			}

			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string bookPath = Path.Combine(wwwRootPath, "images", "book");

                    // Update image if exist
                    if (!string.IsNullOrEmpty(bookVM.Book.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, bookVM.Book.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

					using (var fileStream = new FileStream(Path.Combine(bookPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					bookVM.Book.ImageUrl = @"\images\book\" + fileName;
				}
				else bookVM.Book.ImageUrl = "";

                if (string.IsNullOrEmpty(bookVM.Book.BookId))
                {
					await _unitOfWork.BookRepo.AddAsync(bookVM.Book);
					await _unitOfWork.SaveAsync();
					TempData["success"] = "Book created successfully";
				}
                else
                {
					_unitOfWork.BookRepo.Update(bookVM.Book);
					await _unitOfWork.SaveAsync();
					TempData["success"] = "Book updated successfully";
				}
				
				return RedirectToAction("Index");
			}
			else
			{
				bookVM.CategoryList = (await _unitOfWork.CategoryRepo.GetAllAsync()).Select(
				u => new SelectListItem { Text = u.CategoryName, Value = u.CategoryId.ToString() }
				);
				return View(bookVM);
			}

		}

        [HttpGet]
        public async Task<IActionResult> GetDataFromAPI()
        {
			List<Book> objBookList = (await _unitOfWork.BookRepo.GetAllAsync(IncludeProperties: "Category")).ToList();
            return Json(new {data= objBookList });
		}

		[HttpDelete]
		public async Task<IActionResult> Delete(string? id)
		{
			var book = await _unitOfWork.BookRepo.GetAsync(p=> p.BookId == id);
			if (book == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			//Delete book images
			if (!string.IsNullOrEmpty(book.ImageUrl))
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				var oldImagePath = Path.Combine(wwwRootPath, book.ImageUrl.TrimStart('\\'));
				if (System.IO.File.Exists(oldImagePath))
				{
					System.IO.File.Delete(oldImagePath);
				}
			}
			
			_unitOfWork.BookRepo.Remove(book);
			await _unitOfWork.SaveAsync();

			return Json(new { success = true, message = "Deleted successfully !" });
		}
	}
}
