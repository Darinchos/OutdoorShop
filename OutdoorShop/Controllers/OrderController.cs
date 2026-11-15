using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OutdoorShop.Data;
using OutdoorShop.Models;
using OutdoorShop.Services;
using OutdoorShop.ViewModels;

namespace OutdoorShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CheckoutKey")))
                HttpContext.Session.SetString("CheckoutKey", Guid.NewGuid().ToString("N"));
            var model = new Order();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var profile = await _context.CustomerProfiles.FindAsync(userId);
                model.CustomerName = profile?.FullName ?? User.Identity.Name ?? string.Empty;
                model.Email = User.Identity.Name ?? string.Empty;
                model.Phone = profile?.Phone ?? string.Empty;
                model.Address = profile?.Address ?? string.Empty;
            }
            await PopulateCheckoutSummaryAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([Bind("CustomerName,Email,Phone,Address")] Order order)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCheckoutSummaryAsync();
                return View(order);
            }

            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return RedirectToAction("Index", "Cart");

            List<CartItem>? cart;
            try
            {
                cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            }
            catch (JsonException)
            {
                HttpContext.Session.Remove("Cart");
                TempData["CartMessage"] = "Количката беше невалидна и е изчистена. Добави продуктите отново.";
                return RedirectToAction("Index", "Cart");
            }

            if (cart == null || cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            var checkoutKey = HttpContext.Session.GetString("CheckoutKey");
            if (string.IsNullOrEmpty(checkoutKey))
                return RedirectToAction(nameof(Checkout));

            var existingOrder = await _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(existing => existing.CheckoutKey == checkoutKey);
            if (existingOrder != null)
            {
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("CheckoutKey");
                return RedirectToAction(nameof(Success), new { orderId = existingOrder.Id });
            }

            Dictionary<int, int> quantities;
            try
            {
                quantities = CheckoutOrderBuilder.GetQuantities(cart);
            }
            catch (ArgumentException)
            {
                TempData["CartMessage"] = "Количката съдържа невалидно количество. Провери продуктите и опитай отново.";
                return RedirectToAction("Index", "Cart");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var products = await _context.Products
                .Where(product => quantities.Keys.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);
            if (products.Count != quantities.Count)
            {
                await transaction.RollbackAsync();
                TempData["CartMessage"] = "Един или повече продукти вече не са налични. Провери количката.";
                return RedirectToAction("Index", "Cart");
            }

            List<OrderItem> orderItems;
            try
            {
                orderItems = CheckoutOrderBuilder.BuildItems(quantities, products);
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync();
                TempData["CartMessage"] = "Една или повече стоки вече нямат достатъчна наличност. Количката е обновена.";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var (productId, quantity) in quantities)
            {
                var affected = await _context.Products
                    .Where(product => product.Id == productId && product.Stock >= quantity)
                    .ExecuteUpdateAsync(updates => updates
                        .SetProperty(product => product.Stock, product => product.Stock - quantity));

                if (affected != 1)
                {
                    await transaction.RollbackAsync();
                    await transaction.DisposeAsync();
                    var completed = await _context.Orders.AsNoTracking()
                        .FirstOrDefaultAsync(existing => existing.CheckoutKey == checkoutKey);
                    if (completed != null)
                    {
                        HttpContext.Session.Remove("Cart");
                        HttpContext.Session.Remove("CheckoutKey");
                        return RedirectToAction(nameof(Success), new { orderId = completed.Id });
                    }
                    TempData["CartMessage"] = "Една или повече стоки вече нямат достатъчна наличност. Количката е обновена.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            order.CreatedAt = DateTime.Now;
            order.CheckoutKey = checkoutKey;
            order.CustomerUserId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            order.Status = OrderStatuses.Pending;
            var savedProfile = order.CustomerUserId == null ? null : await _context.CustomerProfiles.FindAsync(order.CustomerUserId);
            if (savedProfile != null)
            {
                if (string.IsNullOrWhiteSpace(order.CustomerName)) order.CustomerName = savedProfile.FullName;
                if (string.IsNullOrWhiteSpace(order.Phone)) order.Phone = savedProfile.Phone ?? string.Empty;
                if (string.IsNullOrWhiteSpace(order.Address)) order.Address = savedProfile.Address ?? string.Empty;
            }
            order.Items = orderItems;
            order.TotalPrice = CheckoutOrderBuilder.CalculateTotal(order.Items);

            _context.Orders.Add(order);
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                await transaction.DisposeAsync();
                _context.ChangeTracker.Clear();
                var completed = await _context.Orders.AsNoTracking()
                    .FirstOrDefaultAsync(existing => existing.CheckoutKey == checkoutKey);
                if (completed == null) throw;
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("CheckoutKey");
                return RedirectToAction(nameof(Success), new { orderId = completed.Id });
            }

            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CheckoutKey");
            return RedirectToAction(nameof(Success), new { orderId = order.Id });
        }

        public IActionResult Success(int orderId)
        {
            ViewData["OrderId"] = orderId;
            return View();
        }

        private async Task PopulateCheckoutSummaryAsync()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem>? cart;
            try
            {
                cart = string.IsNullOrWhiteSpace(cartJson) ? [] : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            }
            catch (JsonException)
            {
                cart = [];
            }

            Dictionary<int, int> quantities;
            try
            {
                quantities = CheckoutOrderBuilder.GetQuantities(cart ?? []);
            }
            catch (ArgumentException)
            {
                quantities = [];
            }

            var products = quantities.Count == 0
                ? new Dictionary<int, Product>()
                : await _context.Products.Where(product => quantities.Keys.Contains(product.Id))
                    .ToDictionaryAsync(product => product.Id);
            ViewBag.CheckoutSummary = quantities
                .Where(item => products.ContainsKey(item.Key))
                .Select(item => new CheckoutSummaryLine(products[item.Key].Name, products[item.Key].Price,
                    item.Value, products[item.Key].Stock))
                .ToList();
        }
    }
}
