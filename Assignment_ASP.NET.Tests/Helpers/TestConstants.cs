namespace Assignment_ASP.NET.Tests.Helpers
{
    /// <summary>
    /// Chứa các hằng số được sử dụng trong tests
    /// </summary>
    public static class TestConstants
    {
        // User Constants
        public const int DefaultUserId = 1;
        public const string DefaultUsername = "testuser";
        public const string DefaultEmail = "test@test.com";
        public const string DefaultAddress = "Test Address";
        public const string DefaultFullName = "Test User";
        public const string DefaultPasswordHash = "hash";
        public const string DefaultPassword = "123456";
        public const string DefaultPhone = "123";

        // Admin Constants
        public const int AdminUserId = 1;
        public const string AdminUsername = "admin";
        public const string AdminEmail = "admin@test.com";
        public const string AdminFullName = "Admin";

        // Role Constants
        public const int AdminRoleId = 1;
        public const int StaffRoleId = 2;
        public const int CustomerRoleId = 3;
        public const string AdminRoleName = "Admin";
        public const string StaffRoleName = "Staff";
        public const string CustomerRoleName = "Customer";

        // Category Constants
        public const int PhoneCategoryId = 1;
        public const int LaptopCategoryId = 2;
        public const string PhoneCategoryName = "Phone";
        public const string LaptopCategoryName = "Laptop";

        // Product Constants
        public const int IPhone14ProductId = 1;
        public const int SamsungS23ProductId = 2;
        public const int MacBookProProductId = 3;
        public const string IPhone14ProductName = "iPhone 14";
        public const string SamsungS23ProductName = "Samsung S23";
        public const string MacBookProProductName = "MacBook Pro";
        public const decimal IPhone14Price = 1000m;
        public const decimal SamsungS23Price = 900m;
        public const decimal MacBookProPrice = 2000m;
        public const string DefaultImageUrl = "/images/test.jpg";

        // Controller Action Names
        public const string IndexAction = "Index";
        public const string DetailsAction = "Details";
        public const string CreateAction = "Create";
        public const string EditAction = "Edit";
        public const string DeleteAction = "Delete";

        // Controller Names
        public const string HomeController = "Home";
        public const string CartController = "Cart";
        public const string ProductsController = "Products";
        public const string CheckoutController = "Checkout";
        public const string AccountController = "Account";
    }
}
