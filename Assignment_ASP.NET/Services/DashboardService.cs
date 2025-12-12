using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;

namespace Assignment_ASP.NET.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var today = now.Date;

            // Calculate Filtered Revenue
            var revenueQuery = _context.Orders.Where(o => o.Status == "Completed");
            string filterLabel = "Tổng doanh thu";

            if (startDate.HasValue)
            {
                revenueQuery = revenueQuery.Where(o => o.OrderDate >= startDate.Value);
                filterLabel = $"Doanh thu từ {startDate:dd/MM/yyyy}";
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                revenueQuery = revenueQuery.Where(o => o.OrderDate <= end);
                if (startDate.HasValue)
                {
                    filterLabel += $" đến {endDate:dd/MM/yyyy}";
                }
                else
                {
                    filterLabel = $"Doanh thu đến {endDate:dd/MM/yyyy}";
                }
            }

            var filteredRevenue = await revenueQuery.SumAsync(o => o.TotalAmount);

            var dashboardData = new DashboardViewModel
            {
                // Thống kê tổng quan
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),

                // Thống kê đơn hàng theo trạng thái
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == "Processing"),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "Completed"),
                CancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled"),

                // Thống kê doanh thu
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount),

                MonthlyRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed" && o.OrderDate >= firstDayOfMonth)
                    .SumAsync(o => o.TotalAmount),

                TodayRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed" && o.OrderDate.Date == today)
                    .SumAsync(o => o.TotalAmount),

                FilteredRevenue = filteredRevenue,
                FilterLabel = filterLabel,

                // Sản phẩm sắp hết hàng
                LowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity < 10),

                // Đơn hàng gần đây (10 đơn mới nhất)
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync(),

                // Top 5 sản phẩm bán chạy
                TopSellingProducts = await _context.OrderDetails
                    .Include(od => od.Product)
                    .GroupBy(od => new { od.ProductID, od.Product.ProductName, od.Product.ImageUrl })
                    .Select(g => new ProductSalesInfo
                    {
                        ProductId = g.Key.ProductID,
                        ProductName = g.Key.ProductName,
                        ImageUrl = g.Key.ImageUrl,
                        TotalSold = g.Sum(od => od.Quantity),
                        Revenue = g.Sum(od => od.Quantity * od.Price)
                    })
                    .OrderByDescending(p => p.TotalSold)
                    .Take(5)
                    .ToListAsync(),

                // Thống kê 12 tháng gần nhất
                MonthlyStatistics = await GetMonthlyStatisticsAsync(12)
            };

            return dashboardData;
        }

        private async Task<List<MonthlyStats>> GetMonthlyStatisticsAsync(int monthCount)
        {
            var stats = new List<MonthlyStats>();
            var now = DateTime.Now;

            for (int i = monthCount - 1; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var firstDay = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                var monthRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed" && 
                               o.OrderDate >= firstDay && 
                               o.OrderDate <= lastDay)
                    .SumAsync(o => o.TotalAmount);

                var monthOrderCount = await _context.Orders
                    .Where(o => o.OrderDate >= firstDay && o.OrderDate <= lastDay)
                    .CountAsync();

                stats.Add(new MonthlyStats
                {
                    Month = targetMonth.Month,
                    Year = targetMonth.Year,
                    Revenue = monthRevenue,
                    OrderCount = monthOrderCount
                });
            }

            return stats;
        }
    }
}
