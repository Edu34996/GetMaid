using System.Text;
using Business.Services;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace WebUi.Controllers // Ensure this matches your project namespace
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplateService _emailTemplates;
        
        public AccountController(IAuthService authService, UserManager<ApplicationUser> userManager, IEmailSender emailSender, EmailTemplateService emailTemplates)
        {
            _authService = authService;
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
        }

        [HttpGet]
        public IActionResult RegisterCustomer() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterCustomer(CustomerRegisterDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.RegisterCustomerAsync(model);
            if (result.IsSuccess) return RedirectToAction("Login");

            foreach (var error in result.Errors) ModelState.AddModelError("", error);
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterWorker() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterWorker(WorkerRegisterDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.RegisterWorkerAsync(model);
            if (result.IsSuccess) return RedirectToAction("Login");

            foreach (var error in result.Errors) ModelState.AddModelError("", error);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.LoginAsync(model);
            if (result.IsSuccess) return RedirectToAction("Index", "Home");

            foreach (var error in result.Errors) ModelState.AddModelError("", error);
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDTO());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Always return same UX to avoid account enumeration
            if (user == null)
            {
                TempData["SuccessMessage"] = "If an account exists for that email, a reset link has been sent.";
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // URL-safe encode token for querystring
            var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { email = model.Email, token = encodedToken },
                protocol: Request.Scheme);

            var subject = "Reset your GetMaid password";
            var body = await _emailTemplates.RenderAsync("ResetPassword.html", new Dictionary<string, string>
            {
                { "RESET_LINK", callbackUrl! }
            });

            await _emailSender.SendEmailAsync(model.Email, "Reset your GetMaid password", body);

            TempData["SuccessMessage"] = "If an account exists for that email, a reset link has been sent.";
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction(nameof(Login));

            var model = new ResetPasswordDTO
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Same behavior to avoid account enumeration
                TempData["SuccessMessage"] = "Password reset completed.";
                return RedirectToAction(nameof(Login));
            }

            // Decode token back to original value
            var decodedBytes = WebEncoders.Base64UrlDecode(model.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your password has been reset successfully.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
    }
}