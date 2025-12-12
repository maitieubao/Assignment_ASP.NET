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

            // Clean up auto-generated products with placeholder images
            var placeholderProducts = context.Products
                .Where(p => p.ImageUrl != null && p.ImageUrl.Contains("placehold.co"))
                .ToList();
            
            if (placeholderProducts.Any())
            {
                context.Products.RemoveRange(placeholderProducts);
                context.SaveChanges();
            }

            // Seed categories if not exist
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
                if (!context.Categories.Any(x => x.CategoryName == c.CategoryName))
                {
                    context.Categories.Add(c);
                }
            }
            context.SaveChanges();

            // Reload categories to get IDs
            var allCategories = context.Categories.ToList();

            // Only seed products if there are very few or none
            if (context.Products.Count() < 5)
            {
                // Add quality sample products with real images
                var sampleProducts = new List<Product>
                {
                    new Product
                    {
                        ProductName = "MacBook Pro 14 inch M3",
                        Description = "Laptop cao cấp với chip Apple M3, màn hình Liquid Retina XDR 14 inch, hiệu năng mạnh mẽ cho công việc chuyên nghiệp.",
                        Price = 49990000,
                        ImageUrl = "https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/mbp14-spacegray-select-202310?wid=904&hei=840&fmt=jpeg&qlt=90&.v=1697230830200",
                        Color = "Space Gray",
                        Size = "14 inch",
                        StockQuantity = 15,
                        CategoryID = allCategories.First(c => c.CategoryName == "Laptop").CategoryID,
                        Manufacturer = "USA",
                        WarrantyPeriod = "12 tháng",
                        KeyFeatures = "Chip Apple M3|RAM 18GB|SSD 512GB|Màn hình Liquid Retina XDR",
                        Specifications = "CPU:Apple M3 8-core|RAM:18GB Unified|Ổ cứng:SSD 512GB|Màn hình:14.2 inch 3024x1964|Pin:Lên đến 17 giờ|Trọng lượng:1.55 kg"
                    },
                    new Product
                    {
                        ProductName = "iPhone 15 Pro Max 256GB",
                        Description = "Điện thoại flagship với chip A17 Pro, camera 48MP, khung Titanium cao cấp, hỗ trợ USB-C.",
                        Price = 34990000,
                        ImageUrl = "https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/iphone-15-pro-max-black-titanium-select?wid=940&hei=1112&fmt=png-alpha&.v=1692846357018",
                        Color = "Black Titanium",
                        Size = "6.7 inch",
                        StockQuantity = 25,
                        CategoryID = allCategories.First(c => c.CategoryName == "Tablet").CategoryID,
                        Manufacturer = "USA",
                        WarrantyPeriod = "12 tháng",
                        KeyFeatures = "Chip A17 Pro|Camera 48MP|Titanium Design|USB-C",
                        Specifications = "CPU:A17 Pro 6-core|RAM:8GB|Bộ nhớ:256GB|Màn hình:6.7 inch Super Retina XDR|Pin:4422 mAh|Trọng lượng:221g"
                    },
                    new Product
                    {
                        ProductName = "Apple Watch Ultra 2",
                        Description = "Đồng hồ thông minh cao cấp với vỏ Titanium, GPS + Cellular, chống nước 100m, pin 36 giờ.",
                        Price = 21990000,
                        ImageUrl = "https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/watch-ultra-2-702702?wid=940&hei=1112&fmt=png-alpha&.v=1693527297631",
                        Color = "Titanium",
                        Size = "49mm",
                        StockQuantity = 10,
                        CategoryID = allCategories.First(c => c.CategoryName == "Smartwatch").CategoryID,
                        Manufacturer = "USA",
                        WarrantyPeriod = "12 tháng",
                        KeyFeatures = "Vỏ Titanium|GPS + Cellular|Chống nước 100m|Pin 36 giờ",
                        Specifications = "Màn hình:1.92 inch OLED|Kết nối:Bluetooth 5.3, WiFi, GPS|Pin:542 mAh|Chống nước:100m WR|Trọng lượng:61.3g"
                    },
                    new Product
                    {
                        ProductName = "Sony Alpha A7 IV",
                        Description = "Máy ảnh mirrorless full-frame 33MP, quay video 4K 60fps, lấy nét tự động AI tiên tiến.",
                        Price = 58990000,
                        ImageUrl = "https://www.sony.com.vn/image/5d02da5df552836db894cead8a68f5f3?fmt=pjpeg&wid=660&bgcolor=FFFFFF&bgc=FFFFFF",
                        Color = "Black",
                        Size = "Full-frame",
                        StockQuantity = 8,
                        CategoryID = allCategories.First(c => c.CategoryName == "Camera").CategoryID,
                        Manufacturer = "Japan",
                        WarrantyPeriod = "24 tháng",
                        KeyFeatures = "Cảm biến 33MP Full-frame|Video 4K 60fps|AI AF|5-axis IBIS",
                        Specifications = "Cảm biến:33MP Full-frame Exmor R|Ống kính:Sony E-mount|Video:4K 60fps / 1080p 120fps|ISO:100-51200|Trọng lượng:658g"
                    },
                    new Product
                    {
                        ProductName = "Sony WH-1000XM5",
                        Description = "Tai nghe over-ear cao cấp với chống ồn chủ động hàng đầu, âm thanh Hi-Res, pin 30 giờ.",
                        Price = 8490000,
                        ImageUrl = "https://www.sony.com.vn/image/8d9a2d323e7ad35a88ec58efde4ab371?fmt=pjpeg&wid=660&bgcolor=FFFFFF&bgc=FFFFFF",
                        Color = "Black",
                        Size = "Over-ear",
                        StockQuantity = 30,
                        CategoryID = allCategories.First(c => c.CategoryName == "Headphone").CategoryID,
                        Manufacturer = "Japan",
                        WarrantyPeriod = "12 tháng",
                        KeyFeatures = "Chống ồn ANC tốt nhất|Hi-Res Audio|Pin 30 giờ|Multipoint",
                        Specifications = "Driver:30mm|Trở kháng:48 Ohm|Đáp ứng tần số:4-40000 Hz|Pin:30 giờ|Kết nối:Bluetooth 5.2, AUX|Trọng lượng:250g"
                    },
                    new Product
                    {
                        ProductName = "JBL Flip 6",
                        Description = "Loa Bluetooth di động chống nước IP67, âm bass mạnh mẽ, pin 12 giờ, kết nối PartyBoost.",
                        Price = 2990000,
                        ImageUrl = "https://www.jbl.com.vn/dw/image/v2/AAUJ_PRD/on/demandware.static/-/Sites-masterCatalog_Harman/default/dw5c79ccdb/JBL_FLIP6_HERO_BLACK_0609.png?sw=537&sfrm=png",
                        Color = "Black",
                        Size = "Portable",
                        StockQuantity = 50,
                        CategoryID = allCategories.First(c => c.CategoryName == "Speaker").CategoryID,
                        Manufacturer = "USA",
                        WarrantyPeriod = "12 tháng",
                        KeyFeatures = "Chống nước IP67|Pin 12 giờ|PartyBoost|JBL Pro Sound",
                        Specifications = "Công suất:30W|Đáp ứng tần số:63-20000 Hz|Pin:4800 mAh|Kết nối:Bluetooth 5.1|Chống nước:IP67|Trọng lượng:550g"
                    },
                    new Product
                    {
                        ProductName = "Logitech G Pro X Superlight 2",
                        Description = "Chuột gaming không dây siêu nhẹ 60g, cảm biến HERO 2, polling rate 2000Hz, pin 95 giờ.",
                        Price = 3690000,
                        ImageUrl = "https://resource.logitechg.com/w_692,c_lpad,ar_4:3,q_auto,f_auto,dpr_1.0/d_transparent.gif/content/dam/gaming/en/products/pro-x2-lightspeed/gallery/pro-x2-lightspeed-gallery-1-black.png?v=1",
                        Color = "Black",
                        Size = "Standard",
                        StockQuantity = 40,
                        CategoryID = allCategories.First(c => c.CategoryName == "Mouse").CategoryID,
                        Manufacturer = "USA",
                        WarrantyPeriod = "24 tháng",
                        KeyFeatures = "Siêu nhẹ 60g|HERO 2 Sensor|2000Hz Polling|Pin 95 giờ",
                        Specifications = "DPI:32000|Số nút:5|Polling rate:2000 Hz|Kết nối:LIGHTSPEED Wireless|Trọng lượng:60g"
                    },
                    new Product
                    {
                        ProductName = "Keychron Q1 Pro",
                        Description = "Bàn phím cơ 75% layout, switch Gateron Jupiter, vỏ nhôm CNC, hỗ trợ Bluetooth và QMK/VIA.",
                        Price = 4990000,
                        ImageUrl = "https://www.keychron.com/cdn/shop/files/Keychron-Q1-Pro-QMK-VIA-wireless-custom-mechanical-keyboard-75-percent-layout-full-aluminum-grey-frame-for-Mac-Windows-Linux-Gateron-Jupiter-brown-switches_1800x1800.jpg?v=1700120023",
                        Color = "Space Gray",
                        Size = "75%",
                        StockQuantity = 20,
                        CategoryID = allCategories.First(c => c.CategoryName == "Keyboard").CategoryID,
                        Manufacturer = "China",
                        WarrantyPeriod = "12 tháng",
                        KeyFeatures = "Vỏ nhôm CNC|Gateron Jupiter|Bluetooth + Wired|QMK/VIA",
                        Specifications = "Loại switch:Gateron Jupiter Brown|Số phím:84|LED:South-facing RGB|Keycap:Double-shot PBT|Kết nối:Bluetooth 5.1, USB-C|Trọng lượng:1.6 kg"
                    },
                    new Product
                    {
                        ProductName = "LG UltraGear 27GP850-B",
                        Description = "Màn hình gaming 27 inch QHD 165Hz, Nano IPS 1ms, G-Sync Compatible, HDR400.",
                        Price = 12990000,
                        ImageUrl = "https://www.lg.com/vn/images/monitors/md07556988/gallery/desktop-01.jpg",
                        Color = "Black",
                        Size = "27 inch",
                        StockQuantity = 15,
                        CategoryID = allCategories.First(c => c.CategoryName == "Monitor").CategoryID,
                        Manufacturer = "Korea",
                        WarrantyPeriod = "24 tháng",
                        KeyFeatures = "QHD 165Hz|Nano IPS 1ms|G-Sync Compatible|HDR400",
                        Specifications = "Kích thước:27 inch|Độ phân giải:2560x1440|Tần số quét:165 Hz|Thời gian phản hồi:1ms|Tấm nền:Nano IPS|Công nghệ:G-Sync, FreeSync Premium"
                    },
                    new Product
                    {
                        ProductName = "SteelSeries Arctis Nova Pro",
                        Description = "Tai nghe gaming cao cấp với Active Noise Cancellation, driver Hi-Fi, DAC ngoài kèm theo.",
                        Price = 8990000,
                        ImageUrl = "https://media.steelseriescdn.com/thumbs/catalog/items/61527/7f0b2fcb92c0424aa3d556ed03d4f2a7.png.500x400_q100_crop-fit_optimize.png",
                        Color = "Black",
                        Size = "Over-ear",
                        StockQuantity = 12,
                        CategoryID = allCategories.First(c => c.CategoryName == "Gaming Gear").CategoryID,
                        Manufacturer = "USA",
                        WarrantyPeriod = "24 tháng",
                        KeyFeatures = "Active Noise Cancellation|Hi-Fi Drivers|GameDAC Gen 2|Infinity Battery System",
                        Specifications = "Driver:40mm Neodymium|Trở kháng:38 Ohm|Đáp ứng tần số:10-40000 Hz|Pin:22 giờ (mỗi pin)|Kết nối:2.4GHz Wireless, Bluetooth, Wired|Trọng lượng:338g"
                    }
                };

                context.Products.AddRange(sampleProducts);
                context.SaveChanges();
            }

            // Add Users
            if (context.Users.Count() < 10)
            {
                var sha256 = SHA256.Create();
                var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("123456"))).Replace("-", "").ToLower();
                var customerRole = context.Roles.FirstOrDefault(r => r.RoleName == "Customer");

                if (customerRole != null)
                {
                    // ... existing customer seeding ...
                    var usersToAdd = new List<User>();
                    // ... (existing code for customers) ...
                     if (!context.Users.Any(u => u.Username == "khachhang1"))
                    {
                        usersToAdd.Add(new User
                        {
                            Username = "khachhang1",
                            PasswordHash = passwordHash,
                            FullName = "Nguyễn Văn An",
                            Email = "an.nguyen@email.com",
                            Address = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                            Phone = "0901234567",
                            RoleID = customerRole.RoleID
                        });
                    }

                    if (!context.Users.Any(u => u.Username == "khachhang2"))
                    {
                        usersToAdd.Add(new User
                        {
                            Username = "khachhang2",
                            PasswordHash = passwordHash,
                            FullName = "Trần Thị Bình",
                            Email = "binh.tran@email.com",
                            Address = "456 Lê Lợi, Quận 3, TP.HCM",
                            Phone = "0907654321",
                            RoleID = customerRole.RoleID
                        });
                    }

                    if (usersToAdd.Any())
                    {
                        context.Users.AddRange(usersToAdd);
                        context.SaveChanges();
                    }
                }

                // Seed Admin User
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                if (adminRole != null)
                {
                    if (!context.Users.Any(u => u.Username == "admin"))
                    {
                         context.Users.Add(new User
                        {
                            Username = "admin",
                            PasswordHash = passwordHash,
                            FullName = "Administrator",
                            Email = "admin@email.com",
                            Address = "Admin Address",
                            Phone = "0000000000",
                            RoleID = adminRole.RoleID
                        });
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
