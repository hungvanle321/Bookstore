using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Bookstore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookstore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork db)
        {
            _unitOfWork = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
		public async Task<IActionResult> Create(string inputValue)
        {
			try
			{
				Category category = new()
                {
                    CategoryName = inputValue
                };
				await _unitOfWork.CategoryRepo.AddAsync(category);
				await _unitOfWork.SaveAsync();
				return Json(new { success = true, message = "Category created successfully!" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "An error occurred: " + ex.Message });
			}
		}

        [HttpPut]
        public async Task<IActionResult> Edit(int? id, string inputValue)
        {
			var category = await _unitOfWork.CategoryRepo.GetAsync(p => p.CategoryId == id);
			if (category == null)
			{
				return Json(new { success = false, message = "Error while editing" });
			}

            category.CategoryName = inputValue;
			_unitOfWork.CategoryRepo.Update(category);
			await _unitOfWork.SaveAsync();
			return Json(new { success = true, message = "Category edited successfully!" });
		}

		[HttpGet]
        public async Task<IActionResult> GetDataFromAPI()
        {
            List<Category> objCategoryList = (await _unitOfWork.CategoryRepo.GetAllAsync()).ToList();
            return Json(new { data = objCategoryList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var category = await _unitOfWork.CategoryRepo.GetAsync(p => p.CategoryId == id);
            if (category == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var books = await _unitOfWork.BookRepo.GetAsync(b => b.CategoryID == id);
            if (books != null)
            {
				return Json(new { success = false, message = "Please remove all books from this category before attempting to delete it" });
			}

            _unitOfWork.CategoryRepo.Remove(category);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Deleted successfully !" });
        }
    }
}

