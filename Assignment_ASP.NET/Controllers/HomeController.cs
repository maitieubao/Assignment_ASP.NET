using Microsoft.AspNetCore.Mvc;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using System.Diagnostics;

namespace Assignment_ASP.NET.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
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

            var (products, totalPages, currentPage, totalItems) = await _productService.GetHomeProductsAsync(
                searchString, categoryId, sortOrder, minPrice, maxPrice, page, pageSize);

            var categories = await _productService.GetCategoriesAsync();

            var viewModel = new HomeIndexViewModel
            {
                Products = products,
                Categories = categories,
                CurrentCategoryId = categoryId,
                CurrentSearchString = searchString,
                CurrentSortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: /Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductWithReviewsAsync(id.Value);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: /Home/Promotions
        public async Task<IActionResult> Promotions()
        {
            var activeCoupons = await _productService.GetActiveCouponsAsync();
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