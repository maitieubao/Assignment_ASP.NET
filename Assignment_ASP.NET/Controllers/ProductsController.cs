using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;

        public ProductsController(IProductService productService, ApplicationDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        // GET: /Products
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? page)
        {
            var (products, totalPages, currentPage) = await _productService.GetProductsAsync(
                searchString, categoryId, page ?? 1);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.Categories = await _productService.GetCategoriesAsync();

            return View(products);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: /Products/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropDownList();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ProductName,Description,Price,Color,Size,StockQuantity,CategoryID")] Product product,
            IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Duplicate Name Check
                if (await _context.Products.AnyAsync(p => p.ProductName == product.ProductName))
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");
                    await PopulateCategoriesDropDownList(product.CategoryID);
                    return View(product);
                }

                await _productService.CreateProductAsync(product, imageFile);
                TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();

            await PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProductID,ProductName,Description,Price,ImageUrl,Color,Size,StockQuantity,CategoryID")] Product product,
            IFormFile? imageFile)
        {
            if (id != product.ProductID) return NotFound();

            if (ModelState.IsValid)
            {
                // Duplicate Name Check
                if (await _context.Products.AnyAsync(p => p.ProductName == product.ProductName && p.ProductID != id))
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");
                    await PopulateCategoriesDropDownList(product.CategoryID);
                    return View(product);
                }

                try
                {
                    await _productService.UpdateProductAsync(product, imageFile);
                    TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    if (await _productService.GetProductByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            await PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProductAsync(id);
            TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categories = await _productService.GetCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", selectedCategory);
        }
    }
}