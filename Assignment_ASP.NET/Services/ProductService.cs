using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment_ASP.NET.Services
{
    public interface IProductService
    {
        Task<(List<Product> Items, int TotalPages, int CurrentPage)> GetProductsAsync(
            string searchString, 
            int? categoryId, 
            int page, 
            int pageSize = 10);
        
        Task<(List<Product> Items, int TotalPages, int CurrentPage, int TotalItems)> GetHomeProductsAsync(
            string searchString,
            int? categoryId,
            string sortOrder,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize = 20);
        
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product?> GetProductWithReviewsAsync(int id);
        Task CreateProductAsync(Product product, IFormFile? imageFile);
        Task UpdateProductAsync(Product product, IFormFile? imageFile);
        Task DeleteProductAsync(int id);
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Coupon>> GetActiveCouponsAsync();
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<(List<Product> Items, int TotalPages, int CurrentPage)> GetProductsAsync(
            string searchString, 
            int? categoryId, 
            int page, 
            int pageSize = 10)
        {
            // Implementation giữ nguyên, chỉ gọi GetHomeProductsAsync với tham số mặc định
            var result = await GetHomeProductsAsync(searchString, categoryId, null, null, null, page, pageSize);
            return (result.Items, result.TotalPages, result.CurrentPage);
        }

        public async Task<(List<Product> Items, int TotalPages, int CurrentPage, int TotalItems)> GetHomeProductsAsync(
            string searchString,
            int? categoryId,
            string sortOrder,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize = 20)
        {
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.Description.Contains(searchString) ||
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

            // Pagination
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            int currentPage = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var products = await productsQuery
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalPages, currentPage, totalItems);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);
        }

        public async Task CreateProductAsync(Product product, IFormFile? imageFile)
        {
            if (imageFile != null)
            {
                product.ImageUrl = await UploadFile(imageFile);
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product, IFormFile? imageFile)
        {
            if (imageFile != null)
            {
                // Delete old image
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteFile(product.ImageUrl);
                }

                // Upload new image
                product.ImageUrl = await UploadFile(imageFile);
            }

            _context.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteFile(product.ImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
        }

        public async Task<List<Coupon>> GetActiveCouponsAsync()
        {
            return await _context.Coupons
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.Now)
                .OrderByDescending(c => c.DiscountPercentage)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithReviewsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.ProductID == id);
        }

        // --- Helper Methods ---
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

            try 
            {
                string fileName = Path.GetFileName(imageUrl);
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                // Log error or ignore if file delete fails
            }
        }
    }
}
