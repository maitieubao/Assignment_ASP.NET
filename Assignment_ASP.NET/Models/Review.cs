using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_ASP.NET.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        public int ProductID { get; set; }
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }

        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
