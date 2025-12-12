using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_ASP.NET.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Ví dụ: "Pending", "Approved"

        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // "COD" hoặc "Bank"

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } // "Pending", "Completed", "Failed"

        // Navigation properties
        public virtual User? User { get; set; }

        // Mối quan hệ: Một đơn hàng có nhiều chi tiết
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}