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
	public class TransactionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public TransactionController(IUnitOfWork db)
        {
            _unitOfWork = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDataFromAPI()
        {
            IEnumerable<Transaction> transactions;

            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                transactions = await _unitOfWork.TransactionRepo.GetAllAsync(IncludeProperties: "OrderHeader");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier).Value;
                transactions = await _unitOfWork.TransactionRepo.GetAllAsync(t => t.OrderHeader.ApplicationUserId == userId, IncludeProperties: "OrderHeader");
            }
            transactions = transactions.OrderByDescending(t => t.TransactionTime);

            return Json(new { data = transactions });
        }

        //[HttpGet]
        //public async Task<IActionResult> ShowTransaction(string id)
        //{
        //	return View(id);
        //}
    }
}
