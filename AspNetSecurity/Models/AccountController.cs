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
                    // https://github.com/aspnet/Identity/blob/dev/samples/IdentitySample.Mvc/Controllers/AccountController.cs#L69
                    //return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
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

                // Full article how to do this:
                // https://docs.microsoft.com/en-us/aspnet/identity/overview/features-api/account-confirmation-and-password-recovery-with-aspnet-identity
                //var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                //var callbackUrl = Url.Action(
                //   "ConfirmEmail", "Account",
                //   new { userId = user.Id, code = code },
                //   protocol: Request.Url.Scheme);

                //await UserManager.SendEmailAsync(user.Id,
                //   "Confirm your account",
                //   "Please confirm your account by clicking this link: <a href=\""
                //                                   + callbackUrl + "\">link</a>");
                //// ViewBag.Link = callbackUrl;   // Used only for initial demo.
                //return View("DisplayEmail");


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


        //Example forgot password functionality:
        // https://docs.microsoft.com/en-us/aspnet/identity/overview/features-api/account-confirmation-and-password-recovery-with-aspnet-identity
        //public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await UserManager.FindByNameAsync(model.Email);
        //        if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
        //        {
        //            // Don't reveal that the user does not exist or is not confirmed
        //            return View("ForgotPasswordConfirmation");
        //        }

        //        var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
        //        var callbackUrl = Url.Action("ResetPassword", "Account",
        //    new { UserId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //        await UserManager.SendEmailAsync(user.Id, "Reset Password",
        //    "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
        //        return View("ForgotPasswordConfirmation");
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        [HttpGet]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return View("LoggedOut");
        }


        // 2fa example code:
        // https://github.com/aspnet/Identity/blob/dev/samples/IdentitySample.Mvc/Controllers/AccountController.cs#L350
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        //{
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}


        //
        // GET: /Account/VerifyCode
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        //{
        //    // Require that the user has already logged in via username/password or external login
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}
    }
}