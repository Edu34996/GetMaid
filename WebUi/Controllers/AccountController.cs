using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebUi.Controllers // Ensure this matches your project namespace
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
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
    }
}