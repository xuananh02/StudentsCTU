// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Elfie.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;


namespace SocialMediaWisLam.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<Profile> _userManager;
        private readonly SignInManager<Profile> _signInManager;
        private readonly SocialMediaWisLamContext _context;

        public IndexModel(
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager,
            SocialMediaWisLamContext context
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
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
        [TempData]
        public string StatusMessage { get; set; }

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
            /// 

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }


            [Display(Name = "Birdthday")]
            public DateTime Birthday { get; set; }

            [Display(Name = "PictureUrl")]
            public string PictureUrl { get; set; }


            [Display(Name = "CoverPictureUrl")]
            public string CoverPictureUrl { get; set; }


            [Display(Name = "About me")]
            public string AboutMe { get; set; }


            [Display(Name = "Gender")]
            public int Gender { get; set; }


            [Display(Name = "City")]
            public string City { get; set; }

            [Display(Name = "Country")]
            public string Country { get; set; }

            [Display(Name = "Street")]
            public string Street { get; set; }

            [Display(Name = "ZipCode")]
            public string ZipCode { get; set; }

            [Display(Name = "country")]
            public List<Location> Locations { get; set; }

            [Display(Name = "userId")]
            public string UserId { get; set; }

            public int LocationId { get; set; }
        }

        private async Task LoadAsync(Profile user)
        {
            var userProfile = await _userManager.GetUserAsync(User);
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var pictureUrl = userProfile.PictureUrl;
            var coverPictureUrl = userProfile.CoverPictureUrl;
            var birthday = userProfile.Birthday;
            var gender = userProfile.Gender;
            var aboutMe = userProfile.AboutMe;

            var locations = await _context.Location.ToListAsync();
            var location = userProfile.LocationOwner;
            var locationId = (location == null) ? 0 : location.Id;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                PictureUrl = pictureUrl,
                CoverPictureUrl = coverPictureUrl,
                AboutMe = aboutMe,
                Gender = gender,
                Birthday = birthday,
                Locations = locations,
                UserId = userProfile.Id,
                LocationId = locationId
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
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
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.Birthday != DateTime.Now)
            {
                user.Birthday = Input.Birthday;
                var setBirthdayResult = await _userManager.UpdateAsync(user);
                if (!setBirthdayResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set birthday.";
                    return RedirectToPage();
                }
            }

            if (Input.AboutMe != user.AboutMe)
            {
                user.AboutMe = Input.AboutMe;
                var setAboutMeResult = await _userManager.UpdateAsync(user);
                if (!setAboutMeResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set about me";
                    return RedirectToPage();
                }
            }

            if (Input.PictureUrl != user.PictureUrl)
            {
                user.PictureUrl = Input.PictureUrl;
                var setAboutMeResult = await _userManager.UpdateAsync(user);
                if (!setAboutMeResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set picture url";
                    return RedirectToPage();
                }
            }

            if (Input.CoverPictureUrl != user.CoverPictureUrl)
            {
                user.CoverPictureUrl = Input.CoverPictureUrl;
                var setAboutMeResult = await _userManager.UpdateAsync(user);
                if (!setAboutMeResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set cover picture url";
                    return RedirectToPage();
                }
            }

            if (Input.Gender != user.Gender)
            {
                user.Gender = Input.Gender;
                var setAboutMeResult = await _userManager.UpdateAsync(user);
                if (!setAboutMeResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set gender";
                    return RedirectToPage();
                }
            }


            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
