using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModel;
using Bookstore.Utility;
using System.Data;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

namespace Bookstore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class BookController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IConfiguration _configuration;
        public BookController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _unitOfWork = db;
            _webHostEnvironment = webHostEnvironment;
			_configuration = configuration;
        }
		[Breadcrumb(Title = "Book Management")]
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> CreateAndEdit(int? id)
        {
			BookEditViewModel bookVM = new()
			{
				CategoryList = (await _unitOfWork.CategoryRepo.GetAllAsync()).Select(
				u => new SelectListItem { Text = u.CategoryName, Value = u.CategoryId.ToString() }
				),
				LanguageList = (await _unitOfWork.LanguageRepo.GetAllAsync()).Select(
				u => new SelectListItem { Text = u.LanguageName, Value = u.LanguageId.ToString() }
				),
				Book = new Book() { PublicationDateUI = DateTime.Now }
			};


            if (id == null)
            {
				//Create
				var managementPage = new MvcBreadcrumbNode("Index", "Book", "Book Management", areaName: "Admin");
				var page = new MvcBreadcrumbNode("CreateAndEdit", "Book", "Create New Book", areaName: "Admin") { Parent = managementPage };
				ViewData["BreadcrumbNode"] = page;
				return View(bookVM);
            }
            else
            {
				var managementPage = new MvcBreadcrumbNode("Index", "Book", "Book Management");
				var page = new MvcBreadcrumbNode("CreateAndEdit", "Book", "Update Book") { Parent = managementPage };
				ViewData["BreadcrumbNode"] = page;
				bookVM.Book = await _unitOfWork.BookRepo.GetAsync(c => c.BookId == id, tracked: true);
                return View(bookVM);
			}
		}

		[HttpPost]
		public IActionResult ValidateFile(IFormFile? file)
		{
			if (file == null || file.Length == 0)
			{
				return Json(new { success = false, error = "Problem when uploading this file. Please try another file." });
			}

			if (!FileImageUploadValidation.IsFileExtensionValid(file, out string ErrorMessage))
			{
				return Json(new { success = false, error = ErrorMessage });
			}

			if (!FileImageUploadValidation.IsFileSignatureValid(file, out ErrorMessage))
			{
				return Json(new { success = false, error = ErrorMessage });
			}

			long fileSizeLimit = _configuration.GetValue<long>("FileSizeLimit");
			if (FileImageUploadValidation.IsFileSizeExceedLimit(file, fileSizeLimit, out ErrorMessage))
			{
				return Json(new { success = false, error = ErrorMessage });
			}

			return Json(new { success = true });
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateAndEdit(BookEditViewModel bookVM, IFormFile? file)
		{
            //Validation
			if (bookVM.Book.OriginPrice < bookVM.Book.DiscountPrice)
			{
				ModelState.AddModelError("Book.DiscountPrice", "The discount price cannot be greater than original");
			}
			
			if (file !=null && (file.Length ==0 || !file.ContentType.Contains("image")))
			{
				ModelState.AddModelError("Book.ImageUrl", "The selected file is not a valid image file");
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

				bookVM.Book.PublicationDate = DateOnly.FromDateTime(bookVM.Book.PublicationDateUI);
				var claimedIdentity = (ClaimsIdentity?)User.Identity;
				var user = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier);

				if (bookVM.Book.BookId == 0)
                {
					bookVM.Book.CreatedBy = user.Value;
					bookVM.Book.CreatedAt = DateTime.Now;
                    bookVM.Book.UpdatedBy = user.Value;
                    bookVM.Book.UpdatedAt = DateTime.Now;
                    await _unitOfWork.BookRepo.AddAsync(bookVM.Book);
					await _unitOfWork.SaveAsync();
					TempData["success"] = "Book created successfully";
				}
                else
                {
					bookVM.Book.UpdatedBy = user.Value;
					bookVM.Book.UpdatedAt = DateTime.Now;
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
				bookVM.LanguageList = (await _unitOfWork.LanguageRepo.GetAllAsync()).Select(
				u => new SelectListItem { Text = u.LanguageName, Value = u.LanguageId.ToString() }
				);
				return View(bookVM);
			}
		}

        [HttpGet]
        public async Task<IActionResult> GetDataFromAPI()
        {
			List<Book> objBookList = (await _unitOfWork.BookRepo.GetAllAsync(IncludeProperties: "Category,Language")).ToList();
            return Json(new {data= objBookList });
		}

		[HttpDelete]
		public async Task<IActionResult> Delete(int? id)
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
