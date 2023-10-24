using Microsoft.AspNetCore.Mvc;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Utility;
using System.Security.Claims;
using Bookstore.Models;
using Microsoft.AspNetCore.Identity;

namespace Bookstore.ViewComponents
{
    public class AvatarViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public AvatarViewComponent(UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimedIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimedIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userInfo = (ApplicationUser)await _userManager.FindByIdAsync(userId);

            return View(model:userInfo.AvatarPath);
        }
    }
}
