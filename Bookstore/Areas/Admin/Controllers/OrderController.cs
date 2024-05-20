using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookstore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork db)
        {
            _unitOfWork = db;
        }
        public IActionResult Index()
        {
            return View();
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
            IEnumerable<OrderHeader> OrderHeaders;

            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                OrderHeaders = await _unitOfWork.OrderHeaderRepo.GetAllAsync(IncludeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier).Value;
                OrderHeaders = await _unitOfWork.OrderHeaderRepo.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "ApplicationUser");
            }
            OrderHeaders = OrderHeaders.OrderByDescending(o => o.OrderDate);

            return Json(new { data = OrderHeaders });
        }

        //      [HttpPut]
        //public async Task<IActionResult> Edit(string orderId)
        //{

        //}
    }
}
