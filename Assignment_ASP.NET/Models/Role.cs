using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_ASP.NET.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key] // Đánh dấu đây là Khóa chính
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tăng
        public int RoleID { get; set; }

        [Required] // Không được để trống
        [StringLength(50)] // Giống nvarchar(50)
        public string RoleName { get; set; }

        // Mối quan hệ: Một vai trò có nhiều người dùng
        public virtual ICollection<User> Users { get; set; }
    }
}