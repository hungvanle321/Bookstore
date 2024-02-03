// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Nodes;

namespace Bookstore.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
            [Required]
			[Display(Name = "Name")]
			public string Name { get; set; }
            public string AvatarPath { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userInfo = await _userManager.FindByIdAsync(user.Id);            

            Username = userInfo.UserName;

            Input = new InputModel
            {
                PhoneNumber = userInfo.PhoneNumber,
                Name = userInfo.Name,
                AvatarPath = userInfo.AvatarPath
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

			var accountPage = new RazorPageBreadcrumbNode("/Identity/Account/Manage", "Account");
			var page = new RazorPageBreadcrumbNode("/Identity/Account/Manage", "Profiles") { Parent = accountPage };
			ViewData["BreadcrumbNode"] = page;

			await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostFileValidationAsync(IFormFile file)
        {

            if (file == null || file.Length == 0)
            {
                return new JsonResult(new { success = false, error = "Problem when uploading this file. Please try another file." });
            }

            if (!FileImageUploadValidation.IsFileExtensionValid(file, out string errorMessage))
            {
                return new JsonResult(new { success = false, error = errorMessage });
            }

            if (!FileImageUploadValidation.IsFileSignatureValid(file, out errorMessage))
            {
                return new JsonResult(new { success = false, error = errorMessage });
            }

            long fileSizeLimit = _configuration.GetValue<long>("FileSizeLimit");
            if (FileImageUploadValidation.IsFileSizeExceedLimit(file, fileSizeLimit, out errorMessage))
            {
                return new JsonResult(new { success = false, error = errorMessage });
            }

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? file)
        {
            var user = (ApplicationUser)await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var isPhoneNumberExist = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);
                if (isPhoneNumberExist!=null && isPhoneNumberExist.UserName != user.UserName)
                {
                    ModelState.AddModelError("Input.PhoneNumber", "This phone number is already registered");
                    await LoadAsync(user);
                    return Page();
                }

                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    ModelState.AddModelError("Input.PhoneNumber", "Unexpected error when trying to set phone number");
                    await LoadAsync(user);
                    return Page();
                }
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string avatarPath = Path.Combine(wwwRootPath, "images", "avatar");

                // Update image if exist
                if (!string.IsNullOrEmpty(user.AvatarPath))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, user.AvatarPath.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(avatarPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                user.AvatarPath = @"\images\avatar\" + fileName;
            }

            user.Name = Input.Name;
            await _userManager.UpdateAsync(user);
            
            await _signInManager.RefreshSignInAsync(user);
            TempData["success"] = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
