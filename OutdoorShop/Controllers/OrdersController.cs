using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutdoorShop.Data;
using OutdoorShop.Models;
using OutdoorShop.Services;

namespace OutdoorShop.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return BadRequest();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var order = await _context.Orders.AsNoTracking().Include(item => item.Items)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (order == null) return NotFound();
            var currentStatus = order.Status;
            if (!OrderStatuses.AllowedTransitions(currentStatus).Contains(status))
            {
                await transaction.RollbackAsync();
                TempData["StatusError"] = "Тази промяна на статуса не е разрешена.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (currentStatus == status)
            {
                await transaction.CommitAsync();
                TempData["StatusMessage"] = "Поръчката вече е с този статус.";
                return RedirectToAction(nameof(Details), new { id });
            }

            foreach (var item in order.Items.Where(item => item.ProductId.HasValue))
            {
                var delta = OrderInventoryPolicy.StockDelta(currentStatus, status, item.Quantity);
                if (delta > 0)
                {
                    var updated = await _context.Products
                        .Where(product => product.Id == item.ProductId && product.Stock <= int.MaxValue - delta)
                        .ExecuteUpdateAsync(updates => updates.SetProperty(product => product.Stock,
                            product => product.Stock + delta));
                    if (updated != 1)
                    {
                        await transaction.RollbackAsync();
                        TempData["StatusError"] = "Наличността е достигнала максималната стойност и не може да бъде възстановена автоматично.";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }
                else if (delta < 0)
                {
                    var required = -delta;
                    var updated = await _context.Products
                        .Where(product => product.Id == item.ProductId && product.Stock >= required)
                        .ExecuteUpdateAsync(updates => updates.SetProperty(product => product.Stock,
                            product => product.Stock - required));
                    if (updated != 1)
                    {
                        await transaction.RollbackAsync();
                        TempData["StatusError"] = "Няма достатъчна наличност, за да възстановиш тази поръчка.";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }
            }

            var statusUpdated = await _context.Orders
                .Where(existing => existing.Id == id && existing.Status == currentStatus)
                .ExecuteUpdateAsync(updates => updates.SetProperty(existing => existing.Status, status));
            if (statusUpdated != 1)
            {
                await transaction.RollbackAsync();
                TempData["StatusError"] = "Поръчката е променена от друг администратор. Презареди страницата и опитай отново.";
                return RedirectToAction(nameof(Details), new { id });
            }
            await transaction.CommitAsync();
            TempData["StatusMessage"] = "Статусът на поръчката е обновен.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
