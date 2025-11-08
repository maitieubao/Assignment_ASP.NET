using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_ASP.NET.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Sẽ lưu mật khẩu đã băm

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(255)] // Đây là trường Address đã sửa
        public string? Address { get; set; } // Dấu ? cho phép giá trị NULL

        [StringLength(20)]
        public string? Phone { get; set; }

        // Khóa ngoại tới bảng Roles
        [ForeignKey("Role")]
        public int RoleID { get; set; }

        // Navigation property (để EF Core hiểu mối quan hệ)
        public virtual Role Role { get; set; }

        // Mối quan hệ: Một người dùng có nhiều đơn hàng
        public virtual ICollection<Order> Orders { get; set; }
    }
}