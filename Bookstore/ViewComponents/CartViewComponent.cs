using Microsoft.AspNetCore.Mvc;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Utility;
using System.Security.Claims;

namespace Bookstore.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimedIdentity = (ClaimsIdentity?)User.Identity;
            var user = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            if (user != null)
            {
                if (HttpContext.Session.GetInt32(StaticDetails.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(StaticDetails.SessionCart,
                        (await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == user.Value)).Count());
                }
                return View(HttpContext.Session.GetInt32(StaticDetails.SessionCart));

            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
