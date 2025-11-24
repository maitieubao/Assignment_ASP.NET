using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Models;
using System.Security.Cryptography;
using System.Text;

namespace Assignment_ASP.NET.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng (DbSet) cho EF Core
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }


        // Cấu hình thêm bằng Fluent API và chèn dữ liệu mẫu (Seeding)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. CẤU HÌNH RÀNG BUỘC (CONSTRAINTS) ---

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();


            // --- 2. CHÈN DỮ LIỆU MẪU (SEED DATA) ---
            #region Seed Data

            // --- Seed Roles ---
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Admin" },
                new Role { RoleID = 2, RoleName = "Employee" },
                new Role { RoleID = 3, RoleName = "Customer" }
            );

            // --- Seed Users ---
            var sha256 = SHA256.Create();
            var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("123456"))).Replace("-", "").ToLower();

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Username = "admin",
                    PasswordHash = passwordHash,
                    FullName = "Admin User",
                    Email = "admin@example.com",
                    Address = "123 Admin St, Da Nang",
                    Phone = "090111222",
                    RoleID = 1
                },
                new User
                {
                    UserID = 2,
                    Username = "employee",
                    PasswordHash = passwordHash,
                    FullName = "Employee User",
                    Email = "employee@example.com",
                    Address = "456 Employee St, Da Nang",
                    Phone = "090333444",
                    RoleID = 2
                },
                new User
                {
                    UserID = 3,
                    Username = "customer",
                    PasswordHash = passwordHash,
                    FullName = "Customer User",
                    Email = "customer@example.com",
                    Address = "789 Customer St, Da Nang",
                    Phone = "090555666",
                    RoleID = 3
                }
            );

            // --- Seed Categories ---
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "Điện thoại" },
                new Category { CategoryID = 2, CategoryName = "Phụ kiện" }
            );

            // --- Seed Products (ĐÃ CẬP NHẬT ĐƯỜNG DẪN ẢNH) ---
            modelBuilder.Entity<Product>().HasData(
                // 8 sản phẩm Điện thoại (CategoryID = 1)
                new Product
                {
                    ProductID = 1,
                    ProductName = "iPhone 14 Pro Max",
                    Description = "Hàng chính hãng VN/A",
                    Price = 27000000,
                    ImageUrl = "/images/iphone_14_promax.png", // <-- Sửa
                    Color = "Deep Purple",
                    Size = "256GB",
                    StockQuantity = 50,
                    CategoryID = 1
                },
                new Product
                {
                    ProductID = 2,
                    ProductName = "Samsung Galaxy S23 Ultra",
                    Description = "Camera 200MP",
                    Price = 25000000,
                    ImageUrl = "/images/galaxy_s23_ultra.png", // <-- Sửa
                    Color = "Green",
                    Size = "256GB",
                    StockQuantity = 40,
                    CategoryID = 1
                },
                new Product
                {
                    ProductID = 3,
                    ProductName = "Oppo Find X5 Pro",
                    Description = "Chip MariSilicon",
                    Price = 18000000,
                    ImageUrl = "/images/oppo_find_x5_pro.png", // <-- Sửa
                    Color = "White",
                    Size = "128GB",
                    StockQuantity = 30,
                    CategoryID = 1
                },
                new Product
                {
                    ProductID = 4,
                    ProductName = "Xiaomi 13",
                    Description = "Hợp tác Leica",
                    Price = 15000000,
                    ImageUrl = "/images/xiaomi_13.png", // <-- Sửa
                    Color = "Black",
                    Size = "128GB",
                    StockQuantity = 20,
                    CategoryID = 1
                },
                new Product
                {
                    ProductID = 5,
                    ProductName = "iPhone 13",
                    Description = "Giá tốt",
                    Price = 17000000,
                    ImageUrl = "/images/iphone_13.png", // <-- Sửa
                    Color = "Blue",
                    Size = "128GB",
                    StockQuantity = 50,
                    CategoryID = 1
                },
                new Product
                {
                    ProductID = 6,
                    ProductName = "Samsung Galaxy Z Fold 4",
                    Description = "Điện thoại gập",
                    Price = 30000000,
                    ImageUrl = "/images/galaxy_zfold4.png", // <-- Sửa
                    Color = "Beige",
                    Size = "256GB",
                    StockQuantity = 15,
                    CategoryID = 1
                },
                new Product
                {
                    ProductID = 7,
                    ProductName = "Google Pixel 7 Pro",
                    Description = "Android thuần",
                    Price = 20000000,
                    ImageUrl = "/images/pixel7_pro.png", // <-- Sửa
                    Color = "Snow",
                    Size = "128GB",
                    StockQuantity = 10,
                    CategoryID = 1
                },
                new Product
                {
                    ProductID = 8,
                    ProductName = "iPhone 14 Plus",
                    Description = "Pin trâu",
                    Price = 22000000,
                    ImageUrl = "/images/iphone14_plus.png", // <-- Sửa
                    Color = "Red",
                    Size = "128GB",
                    StockQuantity = 25,
                    CategoryID = 1
                },

                // 7 sản phẩm Phụ kiện (CategoryID = 2)
                new Product
                {
                    ProductID = 9,
                    ProductName = "AirPods Pro 2",
                    Description = "Chống ồn chủ động",
                    Price = 5500000,
                    ImageUrl = "/images/ipod2_pro.png", // <-- Sửa
                    Color = "White",
                    Size = "N/A",
                    StockQuantity = 100,
                    CategoryID = 2
                },
                new Product
                {
                    ProductID = 10,
                    ProductName = "Củ sạc Apple 20W",
                    Description = "Sạc nhanh",
                    Price = 500000,
                    ImageUrl = "/images/apple_20w.png", // <-- Sửa
                    Color = "White",
                    Size = "20W",
                    StockQuantity = 200,
                    CategoryID = 2
                },
                new Product
                {
                    ProductID = 11,
                    ProductName = "Samsung Galaxy Buds 2",
                    Description = "Thiết kế nhỏ gọn",
                    Price = 2000000,
                    ImageUrl = "/images/galaxy_buds2.png", // <-- Sửa
                    Color = "Black",
                    Size = "N/A",
                    StockQuantity = 80,
                    CategoryID = 2
                },
                new Product
                {
                    ProductID = 12,
                    ProductName = "Ốp lưng iPhone 14 Silicon",
                    Description = "Chính hãng Apple",
                    Price = 1000000,
                    ImageUrl = "/images/op_silicon_iphone14.png", // <-- Sửa
                    Color = "Blue",
                    Size = "14 Pro Max",
                    StockQuantity = 150,
                    CategoryID = 2
                },
                new Product
                {
                    ProductID = 13,
                    ProductName = "Sạc dự phòng Anker 10000mAh",
                    Description = "Siêu nhỏ gọn",
                    Price = 800000,
                    ImageUrl = "/images/anker_10000mah.png", // <-- Sửa
                    Color = "Black",
                    Size = "10000mAh",
                    StockQuantity = 120,
                    CategoryID = 2
                },
                new Product
                {
                    ProductID = 14,
                    ProductName = "Cáp USB-C to Lightning",
                    Description = "Dây dù",
                    Price = 300000,
                    ImageUrl = "/images/c_to_lightning.png", // <-- Sửa
                    Color = "White",
                    Size = "1m",
                    StockQuantity = 300,
                    CategoryID = 2
                },
                new Product
                {
                    ProductID = 15,
                    ProductName = "Chuột Logitech MX Master 3S",
                    Description = "Chuột văn phòng",
                    Price = 2500000,
                    ImageUrl = "/images/logitech_master3s.png", // <-- Sửa
                    Color = "Graphite",
                    Size = "N/A",
                    StockQuantity = 50,
                    CategoryID = 2
                }
            );

            #endregion
        }
    }
}