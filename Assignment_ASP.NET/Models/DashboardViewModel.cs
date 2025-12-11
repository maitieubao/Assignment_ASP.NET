namespace Assignment_ASP.NET.Models
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        
        // Thống kê đơn hàng
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        
        // Thống kê doanh thu
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        
        // Sản phẩm sắp hết hàng (stock < 10)
        public int LowStockProducts { get; set; }
        
        // Đơn hàng gần đây
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        
        // Sản phẩm bán chạy nhất
        public List<ProductSalesInfo> TopSellingProducts { get; set; } = new List<ProductSalesInfo>();
        
        // Thống kê theo tháng (12 tháng gần nhất)
        public List<MonthlyStats> MonthlyStatistics { get; set; } = new List<MonthlyStats>();
    }

    public class ProductSalesInfo
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MonthlyStats
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
