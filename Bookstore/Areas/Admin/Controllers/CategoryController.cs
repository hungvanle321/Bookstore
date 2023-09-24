using Bookstore.DataAccess.Data;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Bookstore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Bookstore.Utility;
using Bookstore.Models;
using Bookstore.DataAccess.Repository.IRepository;

namespace MyWeb.Areas.Admin.Controllers
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
        public async Task<IActionResult> Index()
        {
            List<Category> objCategoryList = (await _unitOfWork.CategoryRepo.GetAllAsync()).ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.CategoryRepo.AddAsync(obj);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            Category category = await _unitOfWork.CategoryRepo.GetAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepo.Update(obj);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            Category category = await _unitOfWork.CategoryRepo.GetAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(string? id)
        {
            Category category = await _unitOfWork.CategoryRepo.GetAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            _unitOfWork.CategoryRepo.Remove(category);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Category removed successfully";
            return RedirectToAction("Index");
        }
    }
}

