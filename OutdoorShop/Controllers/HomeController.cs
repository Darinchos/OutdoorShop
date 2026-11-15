using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutdoorShop.Data;
using OutdoorShop.Models;
using OutdoorShop.ViewModels;

namespace OutdoorShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products.Include(product => product.Category)
                .Where(product => product.Stock > 0)
                .OrderByDescending(product => product.Stock)
                .ThenBy(product => product.Name).Take(4).ToListAsync();
            var discoveryCategories = await _context.Categories
                .Where(category => category.Name == "Раници" || category.Name == "Палатки" || category.Name == "Дрехи" || category.Name == "Аксесоари")
                .OrderBy(category => category.Name == "Палатки" ? 0 : category.Name == "Дрехи" ? 1 : category.Name == "Раници" ? 2 : 3)
                .ToListAsync();
            return View(new HomeViewModel
            {
                FeaturedProducts = featuredProducts,
                DiscoveryCategories = discoveryCategories
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotFoundPage()
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
