namespace Assignment_ASP.NET.Models
{
    // ViewModel này dùng để truyền nhiều model
    // tới View Index của Home
    public class HomeIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        // Dùng để giữ trạng thái lọc/tìm kiếm
        public int? CurrentCategoryId { get; set; }
        public string? CurrentSearchString { get; set; }
        public string? CurrentSortOrder { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}