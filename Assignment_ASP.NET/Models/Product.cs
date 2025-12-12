using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_ASP.NET.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [Required]
        [StringLength(255)]
        public string ProductName { get; set; }

        [Column(TypeName = "ntext")] // Kiểu dữ liệu nvarchar(max)
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Giống trong ERD
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        // Thông tin chi tiết sản phẩm
        [StringLength(200)]
        public string? Manufacturer { get; set; } // Nơi sản xuất

        [StringLength(100)]
        public string? WarrantyPeriod { get; set; } // Thời gian bảo hành

        [Column(TypeName = "ntext")]
        public string? KeyFeatures { get; set; } // Công nghệ nổi bật (JSON hoặc text)

        [Column(TypeName = "ntext")]
        public string? Specifications { get; set; } // Thông số kỹ thuật (JSON hoặc text)

        // Khóa ngoại tới bảng Categories
        [ForeignKey("Category")]
        public int CategoryID { get; set; }

        // Navigation property
        public virtual Category? Category { get; set; }

        // Mối quan hệ: Một sản phẩm có trong nhiều chi tiết đơn hàng
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Mối quan hệ: Một sản phẩm có nhiều đánh giá
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}