using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Bookstore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

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
                var isCategoryExist = await _unitOfWork.CategoryRepo.GetAllAsync(c => c.CategoryName == inputValue);
                if (isCategoryExist.Count() > 0)
                {
                    throw new Exception("This category is already exist");
                }

				var claimedIdentity = (ClaimsIdentity?)User.Identity;
				var user = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier);
				Category category = new()
                {
                    CategoryName = inputValue,
                    CreatedAt = DateTime.Now,
                    CreatedBy = user.Value,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = user.Value
                };
				await _unitOfWork.CategoryRepo.AddAsync(category);
				await _unitOfWork.SaveAsync();
				return Json(new { success = true, message = "Category created successfully!" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
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

			var claimedIdentity = (ClaimsIdentity?)User.Identity;
			var user = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier);
			category.CategoryName = inputValue;
            category.UpdatedAt = DateTime.Now;
            category.UpdatedBy = user.Value;
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

