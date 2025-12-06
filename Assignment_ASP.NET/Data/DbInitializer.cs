using Assignment_ASP.NET.Models;
using System.Security.Cryptography;
using System.Text;

namespace Assignment_ASP.NET.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any products.
            if (context.Products.Count() > 20) // If we have more than the initial seed, assume populated
            {
                return;   // DB has been seeded
            }

            var categories = new Category[]
            {
                new Category { CategoryName = "Laptop" },
                new Category { CategoryName = "Tablet" },
                new Category { CategoryName = "Smartwatch" },
                new Category { CategoryName = "Camera" },
                new Category { CategoryName = "Headphone" },
                new Category { CategoryName = "Speaker" },
                new Category { CategoryName = "Mouse" },
                new Category { CategoryName = "Keyboard" },
                new Category { CategoryName = "Monitor" },
                new Category { CategoryName = "Gaming Gear" }
            };

            foreach (Category c in categories)
            {
                // Check if category exists to avoid duplicates if partial seed exists
                if (!context.Categories.Any(x => x.CategoryName == c.CategoryName))
                {
                    context.Categories.Add(c);
                }
            }
            context.SaveChanges();

            // Reload categories to get IDs
            var allCategories = context.Categories.ToList();

            var products = new List<Product>();
            var random = new Random();
            string[] colors = { "Black", "White", "Silver", "Gold", "Blue", "Red", "Green", "Rose Gold", "Space Gray", "Midnight" };
            string[] sizes = { "Standard", "Large", "Small", "Medium", "XL" };
            string[] manufacturers = { "Vietnam", "China", "Taiwan", "Korea", "Japan", "USA", "Germany" };
            string[] warranties = { "12 tháng", "24 tháng", "36 tháng", "6 tháng" };

            for (int i = 1; i <= 300; i++)
            {
                var category = allCategories[random.Next(allCategories.Count)];
                var productName = $"{category.CategoryName} Model {random.Next(1000, 9999)}";
                
                // Generate detailed specifications based on category
                string keyFeatures = GenerateKeyFeatures(category.CategoryName, random);
                string specifications = GenerateSpecifications(category.CategoryName, random);
                
                products.Add(new Product
                {
                    ProductName = productName,
                    Description = $"Sản phẩm {category.CategoryName} cao cấp với thiết kế hiện đại, tính năng vượt trội. Phù hợp cho mọi nhu cầu sử dụng từ cơ bản đến chuyên nghiệp.",
                    Price = random.Next(100, 5000) * 10000, // 1,000,000 to 50,000,000
                    ImageUrl = $"https://placehold.co/600x400?text={Uri.EscapeDataString(productName)}",
                    Color = colors[random.Next(colors.Length)],
                    Size = sizes[random.Next(sizes.Length)],
                    StockQuantity = random.Next(1, 100),
                    CategoryID = category.CategoryID,
                    Manufacturer = manufacturers[random.Next(manufacturers.Length)],
                    WarrantyPeriod = warranties[random.Next(warranties.Length)],
                    KeyFeatures = keyFeatures,
                    Specifications = specifications
                });
            }


            context.Products.AddRange(products);
            context.SaveChanges();

            // Add Users
            if (context.Users.Count() < 10)
            {
                var sha256 = SHA256.Create();
                var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("123456"))).Replace("-", "").ToLower();
                var customerRole = context.Roles.FirstOrDefault(r => r.RoleName == "Customer");

                if (customerRole != null)
                {
                    var users = new List<User>();
                    for (int i = 1; i <= 20; i++)
                    {
                        users.Add(new User
                        {
                            Username = $"user{i}",
                            PasswordHash = passwordHash,
                            FullName = $"User Number {i}",
                            Email = $"user{i}@example.com",
                            Address = $"{i} Random Street, City",
                            Phone = $"090{random.Next(1000000, 9999999)}",
                            RoleID = customerRole.RoleID
                        });
                    }
                    context.Users.AddRange(users);
                    context.SaveChanges();
                }
            }
        }

        private static string GenerateKeyFeatures(string categoryName, Random random)
        {
            var features = new List<string>();
            
            switch (categoryName)
            {
                case "Laptop":
                    features.Add($"Chip Intel Core i{random.Next(5, 10)} thế hệ {random.Next(10, 14)}");
                    features.Add($"RAM {random.Next(8, 33)} GB DDR{random.Next(4, 6)}");
                    features.Add($"SSD {random.Next(256, 1025)} GB NVMe");
                    features.Add("Màn hình Full HD/4K chống chói");
                    break;
                case "Tablet":
                    features.Add($"Màn hình {random.Next(8, 13)} inch Retina/AMOLED");
                    features.Add($"Chip {(random.Next(2) == 0 ? "Apple M" + random.Next(1, 3) : "Snapdragon " + random.Next(800, 900))}");
                    features.Add("Hỗ trợ bút cảm ứng");
                    features.Add($"Pin {random.Next(6000, 10000)} mAh");
                    break;
                case "Smartwatch":
                    features.Add("Theo dõi sức khỏe 24/7");
                    features.Add("Chống nước IP68/5ATM");
                    features.Add("GPS tích hợp");
                    features.Add($"Pin {random.Next(2, 8)} ngày");
                    break;
                case "Camera":
                    features.Add($"Cảm biến {random.Next(20, 61)} MP");
                    features.Add("Quay video 4K/8K");
                    features.Add("Chống rung quang học");
                    features.Add("Kết nối WiFi/Bluetooth");
                    break;
                case "Headphone":
                    features.Add("Chống ồn chủ động ANC");
                    features.Add("Driver {random.Next(30, 51)} mm");
                    features.Add($"Pin {random.Next(20, 41)} giờ");
                    features.Add("Kết nối Bluetooth 5.0+");
                    break;
                case "Speaker":
                    features.Add($"Công suất {random.Next(10, 101)} W");
                    features.Add("Chống nước IPX7");
                    features.Add("Kết nối đa thiết bị");
                    features.Add($"Pin {random.Next(8, 25)} giờ");
                    break;
                case "Mouse":
                    features.Add($"DPI {random.Next(1000, 16001)}");
                    features.Add("Cảm biến quang học chính xác");
                    features.Add("Thiết kế ergonomic");
                    features.Add("Kết nối không dây/có dây");
                    break;
                case "Keyboard":
                    features.Add("Switch cơ học Cherry MX/Gateron");
                    features.Add("LED RGB 16.8 triệu màu");
                    features.Add("Keycap PBT Double-shot");
                    features.Add("Kết nối USB-C/Wireless");
                    break;
                case "Monitor":
                    features.Add($"Màn hình {random.Next(24, 35)} inch {(random.Next(2) == 0 ? "Full HD" : "4K")}");
                    features.Add($"Tần số quét {random.Next(60, 241)} Hz");
                    features.Add("Công nghệ IPS/VA/TN");
                    features.Add("HDR10, FreeSync/G-Sync");
                    break;
                case "Gaming Gear":
                    features.Add("Thiết kế gaming chuyên nghiệp");
                    features.Add("LED RGB tùy chỉnh");
                    features.Add("Phản hồi nhanh < 1ms");
                    features.Add("Tương thích đa nền tảng");
                    break;
                default:
                    features.Add("Chất lượng cao cấp");
                    features.Add("Thiết kế hiện đại");
                    features.Add("Bền bỉ theo thời gian");
                    features.Add("Dễ dàng sử dụng");
                    break;
            }
            
            return string.Join("|", features);
        }

        private static string GenerateSpecifications(string categoryName, Random random)
        {
            var specs = new List<string>();
            
            switch (categoryName)
            {
                case "Laptop":
                    specs.Add($"CPU:Intel Core i{random.Next(5, 10)} Gen {random.Next(10, 14)}");
                    specs.Add($"RAM:{random.Next(8, 33)} GB DDR{random.Next(4, 6)}");
                    specs.Add($"Ổ cứng:SSD {random.Next(256, 1025)} GB NVMe");
                    specs.Add($"Màn hình:{random.Next(13, 17)} inch Full HD/4K");
                    specs.Add($"Card đồ họa:{(random.Next(2) == 0 ? "Intel Iris Xe" : "NVIDIA RTX " + random.Next(3050, 4090))}");
                    specs.Add($"Trọng lượng:{random.Next(12, 25) / 10.0} kg");
                    break;
                case "Tablet":
                    specs.Add($"Màn hình:{random.Next(8, 13)} inch, {random.Next(1920, 2732)}x{random.Next(1080, 2048)} pixels");
                    specs.Add($"Chip:{(random.Next(2) == 0 ? "Apple M" + random.Next(1, 3) : "Snapdragon " + random.Next(800, 900))}");
                    specs.Add($"RAM:{random.Next(4, 17)} GB");
                    specs.Add($"Bộ nhớ:{random.Next(64, 513)} GB");
                    specs.Add($"Pin:{random.Next(6000, 10000)} mAh");
                    specs.Add($"Trọng lượng:{random.Next(300, 700)} g");
                    break;
                case "Smartwatch":
                    specs.Add($"Màn hình:{random.Next(1, 3)}.{random.Next(1, 10)} inch AMOLED");
                    specs.Add("Kết nối:Bluetooth 5.0, WiFi, GPS");
                    specs.Add($"Pin:{random.Next(200, 500)} mAh");
                    specs.Add("Chống nước:IP68/5ATM");
                    specs.Add($"Trọng lượng:{random.Next(30, 60)} g");
                    break;
                case "Camera":
                    specs.Add($"Cảm biến:{random.Next(20, 61)} MP {(random.Next(2) == 0 ? "Full-frame" : "APS-C")}");
                    specs.Add($"Ống kính:{random.Next(18, 71)}-{random.Next(100, 301)}mm f/{random.Next(18, 56) / 10.0}");
                    specs.Add($"Video:4K {random.Next(30, 121)}fps / 8K {random.Next(24, 61)}fps");
                    specs.Add("ISO:{random.Next(100, 51201)}");
                    specs.Add($"Trọng lượng:{random.Next(400, 1200)} g");
                    break;
                case "Headphone":
                    specs.Add($"Driver:{random.Next(30, 51)} mm");
                    specs.Add($"Trở kháng:{random.Next(16, 65)} Ohm");
                    specs.Add($"Đáp ứng tần số:{random.Next(15, 21)}-{random.Next(20000, 40001)} Hz");
                    specs.Add($"Pin:{random.Next(20, 41)} giờ");
                    specs.Add("Kết nối:Bluetooth 5.0+, AUX 3.5mm");
                    specs.Add($"Trọng lượng:{random.Next(200, 350)} g");
                    break;
                case "Speaker":
                    specs.Add($"Công suất:{random.Next(10, 101)} W");
                    specs.Add($"Đáp ứng tần số:{random.Next(40, 81)}-{random.Next(18000, 20001)} Hz");
                    specs.Add($"Pin:{random.Next(2000, 5001)} mAh");
                    specs.Add("Kết nối:Bluetooth 5.0, AUX, USB");
                    specs.Add("Chống nước:IPX7");
                    specs.Add($"Trọng lượng:{random.Next(500, 2000)} g");
                    break;
                case "Mouse":
                    specs.Add($"DPI:{random.Next(1000, 16001)}");
                    specs.Add($"Số nút:{random.Next(3, 13)}");
                    specs.Add($"Polling rate:{random.Next(125, 1001)} Hz");
                    specs.Add("Kết nối:USB/Wireless 2.4GHz/Bluetooth");
                    specs.Add($"Trọng lượng:{random.Next(60, 130)} g");
                    break;
                case "Keyboard":
                    specs.Add($"Loại switch:{(random.Next(3) == 0 ? "Cherry MX Red" : random.Next(2) == 0 ? "Gateron Brown" : "Kailh Blue")}");
                    specs.Add($"Số phím:{(random.Next(2) == 0 ? "104" : random.Next(2) == 0 ? "87" : "61")}");
                    specs.Add("LED:RGB 16.8 triệu màu");
                    specs.Add("Keycap:PBT Double-shot");
                    specs.Add("Kết nối:USB-C/Wireless");
                    specs.Add($"Trọng lượng:{random.Next(600, 1200)} g");
                    break;
                case "Monitor":
                    specs.Add($"Kích thước:{random.Next(24, 35)} inch");
                    specs.Add($"Độ phân giải:{(random.Next(2) == 0 ? "1920x1080" : "3840x2160")}");
                    specs.Add($"Tần số quét:{random.Next(60, 241)} Hz");
                    specs.Add($"Thời gian phản hồi:{random.Next(1, 6)} ms");
                    specs.Add($"Tấm nền:{(random.Next(3) == 0 ? "IPS" : random.Next(2) == 0 ? "VA" : "TN")}");
                    specs.Add("Công nghệ:HDR10, FreeSync/G-Sync");
                    break;
                case "Gaming Gear":
                    specs.Add("Thiết kế:Ergonomic, Gaming");
                    specs.Add("LED:RGB tùy chỉnh");
                    specs.Add("Phản hồi:< 1ms");
                    specs.Add("Tương thích:PC, Console, Mobile");
                    specs.Add($"Trọng lượng:{random.Next(100, 500)} g");
                    break;
                default:
                    specs.Add("Chất liệu:Cao cấp");
                    specs.Add("Kích thước:Tiêu chuẩn");
                    specs.Add("Màu sắc:Đa dạng");
                    specs.Add("Bảo hành:Chính hãng");
                    break;
            }
            
            return string.Join("|", specs);
        }
    }
}
