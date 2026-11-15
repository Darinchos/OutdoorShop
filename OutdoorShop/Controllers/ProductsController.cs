using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutdoorShop.Data;
using OutdoorShop.Models;

namespace OutdoorShop.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Всички продукти
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, string? search, string? sort)
        {
            var query = _context.Products.AsQueryable();
            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(product => product.CategoryId == categoryId.Value);

            search = search?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(product => product.Name.Contains(search) ||
                    (product.Description != null && product.Description.Contains(search)));

            query = sort switch
            {
                "name_desc" => query.OrderByDescending(product => product.Name),
                "stock" => query.OrderByDescending(product => product.Stock).ThenBy(product => product.Name),
                _ => query.OrderBy(product => product.Name)
            };

            ViewBag.Categories = await _context.Categories.OrderBy(category => category.Name).ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            var products = await query
                .Include(p => p.Category)
                .ToListAsync();

            // SQLite cannot translate ORDER BY for decimal columns. Sort the
            // already-filtered catalog in memory for the price options.
            products = sort switch
            {
                "price_asc" => products.OrderBy(product => product.Price).ThenBy(product => product.Name).ToList(),
                "price_desc" => products.OrderByDescending(product => product.Price).ThenBy(product => product.Name).ToList(),
                _ => products
            };

            return View(products);
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.RelatedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Stock > 0)
                .OrderBy(p => p.Name)
                .Take(4)
                .ToListAsync();

            return View(product);
        }

        // CREATE - показва формата
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .ToListAsync();

            return View();
        }

        // CREATE - записва продукта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            await ValidateCategoryAsync(product.CategoryId);
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories
                    .ToListAsync();

                return View(product);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // EDIT - показва формата
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories
                .ToListAsync();

            return View(product);
        }

        // EDIT - записва промените
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            await ValidateCategoryAsync(product.CategoryId);
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories
                    .ToListAsync();

                return View(product);
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.CategoryId = product.CategoryId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateCategoryAsync(int categoryId)
        {
            if (!await _context.Categories.AnyAsync(category => category.Id == categoryId))
                ModelState.AddModelError(nameof(Product.CategoryId), "Избраната категория не съществува.");
        }

        // DELETE - потвърждение
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // DELETE - изтриване
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
