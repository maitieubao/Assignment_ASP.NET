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
        public async Task<IActionResult> Index(
            string searchString, 
            int? categoryId, 
            string sortOrder,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            const int pageSize = 20;

            var allCategories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            var productsQuery = _context.Products
                                        .Include(p => p.Category)
                                        .AsQueryable();

            // Search filter
            if (!String.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.Category.CategoryName.Contains(searchString)
                );
            }

            // Category filter
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);
            }

            // Price range filter
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // Sorting
            productsQuery = sortOrder switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "name_asc" => productsQuery.OrderBy(p => p.ProductName),
                "name_desc" => productsQuery.OrderByDescending(p => p.ProductName),
                "newest" => productsQuery.OrderByDescending(p => p.ProductID),
                _ => productsQuery.OrderBy(p => p.ProductName)
            };

            // Get total count before pagination
            var totalItems = await productsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            // Apply pagination
            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new HomeIndexViewModel
            {
                Products = products,
                Categories = allCategories,
                CurrentCategoryId = categoryId,
                CurrentSearchString = searchString,
                CurrentSortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
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