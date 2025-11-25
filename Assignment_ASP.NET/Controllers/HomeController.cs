using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using System.Diagnostics;

namespace Assignment_ASP.NET.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var allCategories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            var productsQuery = _context.Products
                                        .Include(p => p.Category)
                                        .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.Category.CategoryName.Contains(searchString)
                );
            }

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);
            }

            var viewModel = new HomeIndexViewModel
            {
                Products = await productsQuery.ToListAsync(),
                Categories = allCategories,
                CurrentCategoryId = categoryId,
                CurrentSearchString = searchString
            };

            return View(viewModel);
        }

        // GET: /Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: /Home/Promotions
        public async Task<IActionResult> Promotions()
        {
            var activeCoupons = await _context.Coupons
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.Now)
                .OrderByDescending(c => c.DiscountPercentage)
                .ToListAsync();

            return View(activeCoupons);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}