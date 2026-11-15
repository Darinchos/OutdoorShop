using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OutdoorShop.Data;
using OutdoorShop.Models;
using OutdoorShop.ViewModels;
using OutdoorShop.Services;

namespace OutdoorShop.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new IdentityUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim()
        };

        if (!await _roleManager.RoleExistsAsync("Customer"))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole("Customer"));
            if (!roleResult.Succeeded && !await _roleManager.RoleExistsAsync("Customer"))
            {
                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
        }

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            var roleAssignment = await _userManager.AddToRoleAsync(user, "Customer");
            if (!roleAssignment.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                foreach (var error in roleAssignment.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email.Trim(), model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Имейлът или паролата са невалидни.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.GetUserAsync(User);
        var profile = await _context.CustomerProfiles.FindAsync(userId);
        return View(new CustomerProfileViewModel
        {
            Email = user?.Email ?? string.Empty,
            FullName = profile?.FullName ?? string.Empty,
            Phone = profile?.Phone ?? user?.PhoneNumber,
            Address = profile?.Address
        });
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CustomerProfileViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.GetUserAsync(User);
        model.Email = user?.Email ?? string.Empty;
        if (!ModelState.IsValid) return View(model);

        var profile = await _context.CustomerProfiles.FindAsync(userId);
        if (profile == null)
        {
            profile = new CustomerProfile { UserId = userId };
            _context.CustomerProfiles.Add(profile);
        }
        profile.FullName = model.FullName.Trim();
        profile.Phone = model.Phone?.Trim();
        profile.Address = model.Address?.Trim();
        if (user != null)
        {
            user.PhoneNumber = profile.Phone;
            var userUpdate = await _userManager.UpdateAsync(user);
            if (!userUpdate.Succeeded)
            {
                foreach (var error in userUpdate.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
        }
        await _context.SaveChangesAsync();
        TempData["ProfileMessage"] = "Профилът е запазен.";
        return RedirectToAction(nameof(Profile));
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var orders = await CustomerOrderOwnership.ForCustomer(_context.Orders.Include(order => order.Items), userId)
            .OrderByDescending(order => order.CreatedAt).ToListAsync();
        return View(orders);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public async Task<IActionResult> MyOrder(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await CustomerOrderOwnership.ForCustomer(_context.Orders.Include(item => item.Items), userId)
            .FirstOrDefaultAsync(item => item.Id == id);
        return order == null ? NotFound() : View(order);
    }
}
