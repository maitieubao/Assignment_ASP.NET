using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Hosting; // Cần cho IWebHostEnvironment
using System.IO;
using Microsoft.AspNetCore.Authorization; // Cần cho Path, FileStream

namespace Assignment_ASP.NET.Controllers
{
     [Authorize(Roles = "Admin,Employee")] // <-- BẠN NÊN THÊM NÀY SAU KHI LÀM XONG LOGIN
    // Chỉ Admin hoặc Employee mới được quản lý Sản phẩm
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Dùng để xử lý upload file

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Products
        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            // Luôn dùng Include() để tải dữ liệu liên quan (Category)
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
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
                .Include(p => p.Category) // Lấy thông tin Category
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: /Products/Create
        // Hiển thị form tạo mới
        public IActionResult Create()
        {
            // Lấy danh sách Categories để tạo dropdown
            PopulateCategoriesDropDownList();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ProductName,Description,Price,Color,Size,StockQuantity,CategoryID")] Product product,
            IFormFile? imageFile) // Tham số để nhận file upload
        {
            if (ModelState.IsValid)
            {
                // Xử lý Upload File
                if (imageFile != null)
                {
                    string imageUrl = await UploadFile(imageFile);
                    product.ImageUrl = imageUrl; // Gán đường dẫn file vào sản phẩm
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu model state lỗi, load lại dropdown
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

            // Load dropdown và chọn sẵn category hiện tại
            PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProductID,ProductName,Description,Price,ImageUrl,Color,Size,StockQuantity,CategoryID")] Product product,
            IFormFile? imageFile) // Tham số để nhận file upload mới (nếu có)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý Upload File MỚI (nếu có)
                    if (imageFile != null)
                    {
                        // Xóa file ảnh CŨ (nếu có)
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            DeleteFile(product.ImageUrl);
                        }

                        // Upload file MỚI
                        string newImageUrl = await UploadFile(imageFile);
                        product.ImageUrl = newImageUrl; // Gán đường dẫn file mới
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
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

            // Nếu model state lỗi, load lại dropdown
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
                // Xóa file ảnh
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteFile(product.ImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // --- HÀM HỖ TRỢ (HELPER METHODS) ---

        // Hàm hỗ trợ load danh sách Category cho Dropdown
        private void PopulateCategoriesDropDownList(object selectedCategory = null)
        {
            var categoriesQuery = from c in _context.Categories
                                  orderby c.CategoryName
                                  select c;

            ViewBag.CategoryID = new SelectList(categoriesQuery.AsNoTracking(),
                                                "CategoryID", "CategoryName",
                                                selectedCategory);
        }

        // Hàm hỗ trợ Upload file
        private async Task<string> UploadFile(IFormFile file)
        {
            // 1. Tạo tên file unique
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

            // 2. Lấy đường dẫn tới thư mục lưu file (ví dụ: wwwroot/images/products)
            // BẠN CẦN TẠO THƯ MỤC NÀY BẰNG TAY TRONG PROJECT
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

            // 3. Tạo đường dẫn file đầy đủ
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. Đảm bảo thư mục tồn tại
            Directory.CreateDirectory(uploadsFolder);

            // 5. Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 6. Trả về đường dẫn TƯƠNG ĐỐI để lưu vào DB
            // (ví dụ: /images/products/ten_file.jpg)
            return Path.Combine("/images", uniqueFileName);
        }

        // Hàm hỗ trợ Xóa file
        private void DeleteFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            // Lấy đường dẫn vật lý đầy đủ của file
            // TrimStart('/') để Path.Combine hoạt động đúng
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}