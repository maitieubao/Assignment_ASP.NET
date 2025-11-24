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
        // Đã cập nhật để nhận searchString và categoryId
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            // --- Logic cho Sidebar ---
            // 1. Lấy tất cả danh mục để hiển thị trên sidebar
            var allCategories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            // --- Logic lọc Sản phẩm ---
            // 2. Bắt đầu câu query sản phẩm
            var productsQuery = _context.Products
                                        .Include(p => p.Category)
                                        .AsQueryable();

            // 3. Lọc theo từ khóa tìm kiếm (từ thanh search)
            if (!String.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.Category.CategoryName.Contains(searchString)
                );
            }

            // 4. Lọc theo danh mục (từ sidebar)
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);
            }

            // 5. Tạo ViewModel và gán dữ liệu vào
            var viewModel = new HomeIndexViewModel
            {
                Products = await productsQuery.ToListAsync(),
                Categories = allCategories,
                CurrentCategoryId = categoryId,
                CurrentSearchString = searchString
            };

            // 6. Trả ViewModel về View
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
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
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