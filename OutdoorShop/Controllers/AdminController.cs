using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutdoorShop.Data;

namespace OutdoorShop.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var model = new AdminDashboardViewModel
        {
            ProductCount = await _context.Products.CountAsync(),
            CustomerCount = await (from userRole in _context.UserRoles
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == "Customer"
                select userRole.UserId).Distinct().CountAsync(),
            OrderCount = await _context.Orders.CountAsync(),
            PendingOrderCount = await _context.Orders.CountAsync(order => order.Status == Models.OrderStatuses.Pending),
            MonthlyRevenue = (await _context.Orders.Where(order => order.CreatedAt >= monthStart &&
                    order.Status != Models.OrderStatuses.Cancelled)
                .Select(order => order.TotalPrice).ToListAsync()).Sum(),
            RecentOrders = await _context.Orders.OrderByDescending(order => order.CreatedAt).Take(6).ToListAsync()
        };
        return View(model);
    }
}

public sealed class AdminDashboardViewModel
{
    public int ProductCount { get; init; }
    public int CustomerCount { get; init; }
    public int OrderCount { get; init; }
    public int PendingOrderCount { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public required IReadOnlyList<Models.Order> RecentOrders { get; init; }
}
