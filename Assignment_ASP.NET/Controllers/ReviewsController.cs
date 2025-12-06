using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Assignment_ASP.NET.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                 return RedirectToAction("Login", "Account");
            }
            var userId = int.Parse(userIdStr);

            var review = new Review
            {
                ProductID = productId,
                UserID = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("Details", "Home", new { id = productId });
        }
    }
}
