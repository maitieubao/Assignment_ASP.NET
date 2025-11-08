using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Models; // <-- Đảm bảo bạn đã using namespace của Models
using System.Security.Cryptography; // Dùng để tạo hash mẫu
using System.Text;

namespace Assignment_ASP.NET.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) // <-- Bạn cần gọi base constructor
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

            // Trong bảng Roles, RoleName là duy nhất
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Trong bảng Users, Username và Email là duy nhất
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Trong bảng Categories, CategoryName là duy nhất
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();


            // --- 2. CHÈN DỮ LIỆU MẪU (SEED DATA) ---
            #region Seed Data

            // --- Seed Roles (Đáp ứng 3 loại người dùng) ---
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Admin" },
                new Role { RoleID = 2, RoleName = "Employee" },
                new Role { RoleID = 3, RoleName = "Customer" }
            );

            // --- Seed Users (Đáp ứng 3 người dùng) ---
            // LƯU Ý: Mật khẩu "123456" đã được băm bằng SHA256.
            // Bạn sẽ cần dùng logic băm SHA256 tương tự trong chức năng Đăng nhập.
            var sha256 = SHA256.Create();
            var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("123456"))).Replace("-", "").ToLower();
            // Hash for "123456" = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"

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
                    RoleID = 1 // Admin
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
                    RoleID = 2 // Employee
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
                    RoleID = 3 // Customer
                }
            );

            // --- Seed Categories (Đáp ứng 2 danh mục) ---
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "Điện thoại" },
                new Category { CategoryID = 2, CategoryName = "Phụ kiện" }
            );

            // --- Seed Products (Đáp ứng 15 sản phẩm) ---
            modelBuilder.Entity<Product>().HasData(
                // 8 sản phẩm Điện thoại (CategoryID = 1)
                new Product
                {
                    ProductID = 1,
                    ProductName = "iPhone 14 Pro Max",
                    Description = "Hàng chính hãng VN/A",
                    Price = 27000000,
                    ImageUrl = "/images/iphone14.jpg",
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
                    ImageUrl = "/images/s23ultra.jpg",
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
                    ImageUrl = "/images/oppo.jpg",
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
                    ImageUrl = "/images/xiaomi13.jpg",
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
                    ImageUrl = "/images/iphone13.jpg",
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
                    ImageUrl = "/images/zfold4.jpg",
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
                    ImageUrl = "/images/pixel7.jpg",
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
                    ImageUrl = "/images/iphone14plus.jpg",
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
                    ImageUrl = "/images/airpods.jpg",
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
                    ImageUrl = "/images/sac20w.jpg",
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
                    ImageUrl = "/images/buds2.jpg",
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
                    ImageUrl = "/images/oplung.jpg",
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
                    ImageUrl = "/images/anker.jpg",
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
                    ImageUrl = "/images/cap.jpg",
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
                    ImageUrl = "/images/logitech.jpg",
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