using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetSecurity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSecurity.Models
{
    public class AccountController : Controller
    {
        private readonly UserManager<ConfArchUser> _userManager;
        private readonly SignInManager<ConfArchUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ConfArchUser> userManager,
            SignInManager<ConfArchUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result =
                    await
                        _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                            lockoutOnFailure: false);
                if (result.Succeeded)
                    return RedirectToLocal(returnUrl);
                if (result.RequiresTwoFactor)
                {
                    //
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
            return View(model);
        }

        /// <summary>
        /// This method prevents redirection attacks:
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Conference");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Error");

            var user = new ConfArchUser
            {
                UserName = model.Email,
                Email = model.Email,
                BirthDate = model.BirthDate
            };
            var result = await _userManager.CreateAsync(
                user, model.Password);

            if (!await _roleManager.RoleExistsAsync("Organizer"))
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = "Organizer" });
            }

            if (!await _roleManager.RoleExistsAsync("Speaker"))
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = "Speaker" });
            }

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _userManager.AddClaimAsync(user, new Claim("technology", model.Technology));

                return View("RegistrationConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("error", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return View("LoggedOut");
        }
    }
}