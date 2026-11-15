using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutdoorShop.Data;
using OutdoorShop.Models;

namespace OutdoorShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Показва количката
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();

            if (cart.Count > 0)
            {
                var productIds = cart.Select(item => item.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(product => productIds.Contains(product.Id))
                    .ToDictionaryAsync(product => product.Id);

                foreach (var item in cart.ToList())
                {
                    if (!products.TryGetValue(item.ProductId, out var product) || product.Stock <= 0)
                    {
                        cart.Remove(item);
                        continue;
                    }

                    item.ProductName = product.Name;
                    item.Price = product.Price;
                    item.ImageUrl = product.ImageUrl;
                    item.Quantity = Math.Min(item.Quantity, product.Stock);
                }

                SaveCart(cart);
            }

            return View(cart);
        }

        // Добавя продукт
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (quantity < 1 || quantity > 1000)
            {
                TempData["CartMessage"] = "Избраното количество е невалидно.";
                return RedirectToAction(nameof(Index));
            }
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);

            var currentQuantity = existingItem?.Quantity ?? 0;
            if ((long)currentQuantity + quantity > product.Stock)
            {
                TempData["CartMessage"] = "Няма достатъчно наличност за това количество.";
                return RedirectToAction(nameof(Index));
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Увеличава количеството
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Increase(int productId)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null || item.Quantity >= product.Stock)
                {
                    TempData["CartMessage"] = "Няма достатъчно наличност за това количество.";
                }
                else
                {
                    item.Quantity++;
                    item.ProductName = product.Name;
                    item.Price = product.Price;
                    item.ImageUrl = product.ImageUrl;
                }
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Намалява количеството
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrease(int productId)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Премахва продукта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Взима количката от Session
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }
            catch (JsonException)
            {
                HttpContext.Session.Remove("Cart");
                TempData["CartMessage"] = "Количката беше невалидна и е изчистена. Добави продуктите отново.";
                return new List<CartItem>();
            }
        }

        // Запазва количката в Session
        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);

            HttpContext.Session.SetString("Cart", cartJson);
        }
    }
}
