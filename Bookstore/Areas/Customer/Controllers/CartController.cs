using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModel;
using Bookstore.Utility;
using System;
using System.Security.Claims;
using SmartBreadcrumbs.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

		[Breadcrumb(Title = "Cart")]
		public async Task<IActionResult> Index()
        {
            var claimedIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shoppingCartViewModel = new ShoppingCartViewModel()
            {
                ShoppingCarts = await _unitOfWork.ShoppingCartRepo.GetAllAsync(u => u.ApplicationUserId == userId,
                IncludeProperties: "Book,Book.Category"),
            };

            return View(shoppingCartViewModel);
        }

		[Breadcrumb(FromAction = "Index", Title = "Checkout")]
		public async Task<IActionResult> Checkout()
        {
			var claimedIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var user = await _unitOfWork.ApplicationUserRepo.GetAsync(u => u.Id == userId);
			string? jsonData = Request.Cookies["jsonData"];

			var viewModel = new OrderViewModel()
            {
				jsonData = jsonData,
				ShipEmail = user.Email,
				ShipPhoneNumber = user.PhoneNumber,
				ShipName = user.Name
            };

			JObject jsonObject = JObject.Parse(jsonData);

			foreach (var property in jsonObject)
			{
				JArray valueArray = (JArray)property.Value;

				int bookId = valueArray[0].Value<int>();
				int count = valueArray[1].Value<int>();

				ShoppingCart shoppingCart = new()
				{
					BookId = bookId,
					Book = await _unitOfWork.BookRepo.GetAsync(b=>b.BookId == bookId),
					ApplicationUserId = userId,
					Count = count
				};
				viewModel.ChosenBooks.Add(shoppingCart);
			}

			return View(viewModel);
		}

        [HttpPost]
		public async Task<IActionResult> CartCountDecrease(string applicationUserId, int bookId)
		{		
			var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(u => u.ApplicationUserId == applicationUserId && u.BookId == bookId, tracked: true);
            if (cartFromDb.Count > 1)
            {
                cartFromDb.Count--;
                _unitOfWork.ShoppingCartRepo.Update(cartFromDb);
				await _unitOfWork.SaveAsync();
                var price = (await _unitOfWork.BookRepo.GetAsync(b => b.BookId == bookId)).DiscountPrice;
				return Json(new { success = true, subTotal = cartFromDb.Count * price });
			}
            else
            {
				HttpContext.Session.SetInt32(StaticDetails.SessionCart,
		        (await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == cartFromDb.ApplicationUserId)).Count() - 1);
				_unitOfWork.ShoppingCartRepo.Remove(cartFromDb);
				await _unitOfWork.SaveAsync();
				return Json(new { success = true });
			}
			
		}

        [HttpPost]
        public async Task<IActionResult> CartCountIncrease(string applicationUserId, int bookId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(u => u.ApplicationUserId == applicationUserId && u.BookId == bookId);
            
            cartFromDb.Count++;
            _unitOfWork.ShoppingCartRepo.Update(cartFromDb);
            await _unitOfWork.SaveAsync();

			var price = (await _unitOfWork.BookRepo.GetAsync(b => b.BookId == bookId)).DiscountPrice;
			return Json(new { success = true, subTotal = cartFromDb.Count * price });
		}

        [HttpDelete]
        public async Task<IActionResult> RemoveCart(string applicationUserId, int bookId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(u => u.ApplicationUserId == applicationUserId && u.BookId == bookId, tracked: true);
            
			HttpContext.Session.SetInt32(StaticDetails.SessionCart,
					(await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == cartFromDb.ApplicationUserId)).Count()-1);
			_unitOfWork.ShoppingCartRepo.Remove(cartFromDb);
			await _unitOfWork.SaveAsync();
			return Json(new { success = true });
		}
    }
}
