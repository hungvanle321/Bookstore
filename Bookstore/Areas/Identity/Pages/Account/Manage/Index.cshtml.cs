// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly CountryService _countryService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            CountryService countryService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _countryService = countryService;
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
            public string Street { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            [Display(Name ="Postal Code")]
            public string PostalCode { get; set; }
            public string Country { get; set; }
            public List<SelectListItem> CountryList { get; set; }
            public List<SelectListItem> StateList { get; set; }
            public List<SelectListItem> CityList { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userInfo = await _userManager.FindByIdAsync(user.Id);            

            Username = userInfo.UserName;

            var countries = new List<SelectListItem>();
            foreach (var  country in await _countryService.GetCountries())
                countries.Add(new SelectListItem { Text = country, Value = country });

            var states = new List<SelectListItem>();
            if (userInfo.Country != null)
            {
                foreach (var state in await _countryService.GetStates(userInfo.Country))
                    states.Add(new SelectListItem { Text = state, Value = state });
            }

            var cities = new List<SelectListItem>();
            if (userInfo.State != null)
            {
                foreach (var city in await _countryService.GetCities(userInfo.Country, userInfo.State))
                    cities.Add(new SelectListItem { Text = city, Value = city });
            }

            Input = new InputModel
            {
                PhoneNumber = userInfo.PhoneNumber,
                Name = userInfo.Name,
                AvatarPath = userInfo.AvatarPath,
                Street = userInfo.Street,
                PostalCode = userInfo.PostalCode,
                CountryList = countries,
                StateList = states,
                CityList = cities,
                Country = userInfo.Country,
                State = userInfo.State,
                City = userInfo.City
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

        public async Task<IActionResult> OnPostStatesAsync(string country)
        {
            var states = await _countryService.GetStates(country);
            return new JsonResult(states);
        }

        public async Task<IActionResult> OnPostCitiesAsync(string country, string state)
        {
            var cities = await _countryService.GetCities(country, state);
            return new JsonResult(cities);
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
            user.Street = Input.Street;
            user.City = Input.City;
            user.State = Input.State;
            user.Country = Input.Country;
            user.PostalCode = Input.PostalCode;
            await _userManager.UpdateAsync(user);
            
            await _signInManager.RefreshSignInAsync(user);
            TempData["success"] = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
