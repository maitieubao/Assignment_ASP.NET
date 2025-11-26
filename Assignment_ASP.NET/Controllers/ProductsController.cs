using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Products - With search, filter, and pagination
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? page)
        {
            const int pageSize = 10;
            int pageNumber = page ?? 1;

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Search by name or description
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => 
                    p.ProductName.Contains(searchString) || 
                    p.Description.Contains(searchString));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);
            }

            // Calculate pagination
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Get paginated results
            var products = await productsQuery
                .OrderBy(p => p.ProductName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass data to view
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            return View(products);
        }

        // GET: /Products/Details/5
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

        // GET: /Products/Create
        public IActionResult Create()
        {
            PopulateCategoriesDropDownList();
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
                // Upload image if provided
                if (imageFile != null)
                {
                    string imageUrl = await UploadFile(imageFile);
                    product.ImageUrl = imageUrl;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProductID,ProductName,Description,Price,ImageUrl,Color,Size,StockQuantity,CategoryID")] Product product,
            IFormFile? imageFile)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle new image upload
                    if (imageFile != null)
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            DeleteFile(product.ImageUrl);
                        }

                        // Upload new image
                        string newImageUrl = await UploadFile(imageFile);
                        product.ImageUrl = newImageUrl;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductID == product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Delete image file
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteFile(product.ImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // --- HELPER METHODS ---

        private void PopulateCategoriesDropDownList(object selectedCategory = null)
        {
            var categoriesQuery = from c in _context.Categories
                                  orderby c.CategoryName
                                  select c;

            ViewBag.CategoryID = new SelectList(categoriesQuery.AsNoTracking(),
                                                "CategoryID", "CategoryName",
                                                selectedCategory);
        }

        private async Task<string> UploadFile(IFormFile file)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            Directory.CreateDirectory(uploadsFolder);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName;
        }

        private void DeleteFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            string fileName = Path.GetFileName(imageUrl);
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}