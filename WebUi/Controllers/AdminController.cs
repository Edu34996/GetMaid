using System.Security.Claims;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebUi.Controllers;

public class AccountSwitcherUserItemVM
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = "User";
}

[Authorize]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IHostEnvironment env,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
        _config = config;
    }

    private bool IsSwitcherEnabled()
        => _env.IsDevelopment() &&
           _config.GetValue<bool>("TestingAuth:EnableAccountSwitcher");

    [HttpGet]
    public async Task<IActionResult> AccountSwitcher(string? q = null)
    {
        if (!CanUseSwitcher()) return Forbid();
        if (!IsSwitcherEnabled()) return NotFound();

        var users = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim().ToLower();
            users = users.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(q)) ||
                (u.FirstName + " " + u.LastName).ToLower().Contains(q));
        }

        var list = users
            .OrderBy(u => u.Email)
            .Take(200) // safeguard
            .ToList();

        var vm = new List<AccountSwitcherUserItemVM>();
        foreach (var u in list)
        {
            var roles = await _userManager.GetRolesAsync(u);
            vm.Add(new AccountSwitcherUserItemVM
            {
                Id = u.Id,
                Email = u.Email ?? "(no email)",
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Role = roles.FirstOrDefault() ?? "User"
            });
        }

        ViewBag.Query = q ?? string.Empty;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssumeUser(string userId)
    {
        if (!CanUseSwitcher()) return Forbid();
        if (!IsSwitcherEnabled()) return NotFound();

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminId)) return Unauthorized();

        var target = await _userManager.FindByIdAsync(userId);
        if (target == null) return BadRequest("User not found.");

        // Optional hard-stop: don't allow chaining assumed sessions
        if (User.HasClaim(c => c.Type == "IsAssumedSession" && c.Value == "true"))
            return BadRequest("Already in an assumed session. Return to admin first.");

        await _signInManager.SignOutAsync();

        var extraClaims = new List<Claim>
        {
            new("AssumedByAdminId", adminId),
            new("IsAssumedSession", "true")
        };

        await _signInManager.SignInWithClaimsAsync(target, isPersistent: false, extraClaims);

        TempData["SuccessMessage"] = $"Now acting as {target.Email}.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnToAdmin()
    {
        if (!CanUseSwitcher()) return Forbid();
        if (!IsSwitcherEnabled()) return NotFound();

        var assumedByAdminId = User.FindFirstValue("AssumedByAdminId");
        if (string.IsNullOrWhiteSpace(assumedByAdminId)) return BadRequest("No admin context.");

        var admin = await _userManager.FindByIdAsync(assumedByAdminId);
        if (admin == null) return BadRequest("Original admin not found.");

        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(admin, isPersistent: false);

        TempData["SuccessMessage"] = $"Returned to admin account {admin.Email}.";
        return RedirectToAction(nameof(AccountSwitcher));
    }
    
    private bool CanUseSwitcher()
    {
        // Real admin OR an assumed session that carries original admin id
        return User.IsInRole("Admin") || User.HasClaim(c => c.Type == "AssumedByAdminId");
    }
}