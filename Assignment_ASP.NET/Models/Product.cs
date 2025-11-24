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

        // Khóa ngoại tới bảng Categories
        [ForeignKey("Category")]
        public int CategoryID { get; set; }

        // Navigation property
        public virtual Category Category { get; set; }

        // Mối quan hệ: Một sản phẩm có trong nhiều chi tiết đơn hàng
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // Mối quan hệ: Một sản phẩm có nhiều đánh giá
        public virtual ICollection<Review> Reviews { get; set; }
    }
}