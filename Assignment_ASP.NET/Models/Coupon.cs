using System;
using System.ComponentModel.DataAnnotations;

namespace Assignment_ASP.NET.Models
{
    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Range(0, 100)]
        public int DiscountPercentage { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
