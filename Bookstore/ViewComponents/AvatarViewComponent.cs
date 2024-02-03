using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bookstore.Models;
using Microsoft.AspNetCore.Identity;

namespace Bookstore.ViewComponents
{
	public class AvatarViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public AvatarViewComponent(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimedIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimedIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userInfo = await _userManager.FindByIdAsync(userId);

            return View(model:userInfo.AvatarPath);
        }
    }
}
