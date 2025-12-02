using Assignment_ASP.NET.Models;
using System.Security.Cryptography;
using System.Text;

namespace Assignment_ASP.NET.Tests.Helpers
{
    /// <summary>
    /// Builder pattern để tạo test data một cách linh hoạt và dễ đọc
    /// </summary>
    public static class TestDataBuilder
    {
        #region User Builders

        /// <summary>
        /// Tạo user mặc định cho testing
        /// </summary>
        public static User CreateDefaultUser()
        {
            return new User
            {
                UserID = TestConstants.DefaultUserId,
                Username = TestConstants.DefaultUsername,
                Email = TestConstants.DefaultEmail,
                Address = TestConstants.DefaultAddress,
                FullName = TestConstants.DefaultFullName,
                PasswordHash = TestConstants.DefaultPasswordHash,
                Phone = TestConstants.DefaultPhone,
                RoleID = TestConstants.CustomerRoleId
            };
        }

        /// <summary>
        /// Tạo admin user với password hash thực
        /// </summary>
        public static User CreateAdminUser()
        {
            return new User
            {
                UserID = TestConstants.AdminUserId,
                Username = TestConstants.AdminUsername,
                Email = TestConstants.AdminEmail,
                Address = TestConstants.DefaultAddress,
                FullName = TestConstants.AdminFullName,
                PasswordHash = HashPassword(TestConstants.DefaultPassword),
                Phone = TestConstants.DefaultPhone,
                RoleID = TestConstants.AdminRoleId
            };
        }

        /// <summary>
        /// Tạo user tùy chỉnh
        /// </summary>
        public static User CreateUser(int userId, string username, string email, int roleId)
        {
            return new User
            {
                UserID = userId,
                Username = username,
                Email = email,
                Address = TestConstants.DefaultAddress,
                FullName = username,
                PasswordHash = TestConstants.DefaultPasswordHash,
                Phone = TestConstants.DefaultPhone,
                RoleID = roleId
            };
        }

        #endregion

        #region Role Builders

        /// <summary>
        /// Tạo danh sách các roles mặc định
        /// </summary>
        public static List<Role> CreateDefaultRoles()
        {
            return new List<Role>
            {
                new Role { RoleID = TestConstants.AdminRoleId, RoleName = TestConstants.AdminRoleName },
                new Role { RoleID = TestConstants.StaffRoleId, RoleName = TestConstants.StaffRoleName },
                new Role { RoleID = TestConstants.CustomerRoleId, RoleName = TestConstants.CustomerRoleName }
            };
        }

        #endregion

        #region Category Builders

        /// <summary>
        /// Tạo danh sách categories mặc định
        /// </summary>
        public static List<Category> CreateDefaultCategories()
        {
            return new List<Category>
            {
                new Category { CategoryID = TestConstants.PhoneCategoryId, CategoryName = TestConstants.PhoneCategoryName },
                new Category { CategoryID = TestConstants.LaptopCategoryId, CategoryName = TestConstants.LaptopCategoryName }
            };
        }

        /// <summary>
        /// Tạo category tùy chỉnh
        /// </summary>
        public static Category CreateCategory(int categoryId, string categoryName)
        {
            return new Category
            {
                CategoryID = categoryId,
                CategoryName = categoryName
            };
        }

        #endregion

        #region Product Builders

        /// <summary>
        /// Tạo danh sách products mặc định
        /// </summary>
        public static List<Product> CreateDefaultProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    ProductID = TestConstants.IPhone14ProductId,
                    ProductName = TestConstants.IPhone14ProductName,
                    CategoryID = TestConstants.PhoneCategoryId,
                    Price = TestConstants.IPhone14Price,
                    ImageUrl = TestConstants.DefaultImageUrl
                },
                new Product
                {
                    ProductID = TestConstants.SamsungS23ProductId,
                    ProductName = TestConstants.SamsungS23ProductName,
                    CategoryID = TestConstants.PhoneCategoryId,
                    Price = TestConstants.SamsungS23Price,
                    ImageUrl = TestConstants.DefaultImageUrl
                },
                new Product
                {
                    ProductID = TestConstants.MacBookProProductId,
                    ProductName = TestConstants.MacBookProProductName,
                    CategoryID = TestConstants.LaptopCategoryId,
                    Price = TestConstants.MacBookProPrice,
                    ImageUrl = TestConstants.DefaultImageUrl
                }
            };
        }

        /// <summary>
        /// Tạo product tùy chỉnh
        /// </summary>
        public static Product CreateProduct(int productId, string productName, int categoryId, decimal price)
        {
            return new Product
            {
                ProductID = productId,
                ProductName = productName,
                CategoryID = categoryId,
                Price = price,
                ImageUrl = TestConstants.DefaultImageUrl
            };
        }

        #endregion

        #region Cart Item Builders

        /// <summary>
        /// Tạo cart item
        /// </summary>
        public static CartItem CreateCartItem(int productId, int quantity, decimal price)
        {
            return new CartItem
            {
                ProductID = productId,
                Quantity = quantity,
                Price = price
            };
        }

        /// <summary>
        /// Tạo danh sách cart items mặc định
        /// </summary>
        public static List<CartItem> CreateDefaultCartItems()
        {
            return new List<CartItem>
            {
                CreateCartItem(TestConstants.IPhone14ProductId, 1, TestConstants.IPhone14Price)
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Hash password sử dụng SHA256 (giống như trong ứng dụng thực)
        /// </summary>
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        #endregion
    }
}
