using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Constants;
using Assignment_ASP.NET.Helpers;

namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = Roles.Customer)]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public CartController(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        /// <summary>
        /// GET: /Cart
        /// Hiển thị trang giỏ hàng
        /// </summary>
        public IActionResult Index()
        {
            var cart = _cartService.GetCartItems(HttpContext);
            var coupon = HttpContext.Session.Get<Coupon>(SessionKeys.Coupon);

            decimal totalAmount = _cartService.GetCartTotal(HttpContext);
            decimal discountAmount = 0;

            if (coupon != null)
            {
                if (coupon.ExpiryDate < DateTime.Now || !coupon.IsActive)
                {
                    HttpContext.Session.Remove(SessionKeys.Coupon);
                    coupon = null;
                }
                else
                {
                    discountAmount = totalAmount * coupon.DiscountPercentage / 100;
                }
            }

            ViewBag.TotalAmount = totalAmount;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.FinalAmount = totalAmount - discountAmount;
            ViewBag.AppliedCoupon = coupon;

            return View(cart);
        }

        /// <summary>
        /// POST: /Cart/Add
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var success = await _cartService.AddToCartAsync(HttpContext, productId, quantity);

            if (!success)
            {
                return NotFound("Sản phẩm không tồn tại");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Cart/Remove
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            _cartService.RemoveFromCart(HttpContext, productId);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Cart/Update
        /// Cập nhật số lượng sản phẩm
        /// </summary>
        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            _cartService.UpdateQuantity(HttpContext, productId, quantity);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Cart/Clear
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult Clear()
        {
            _cartService.ClearCart(HttpContext);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Cart/ApplyCoupon
        /// Áp dụng mã giảm giá
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                TempData["CouponError"] = "Vui lòng nhập mã giảm giá.";
                return RedirectToAction("Index");
            }

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);

            if (coupon == null)
            {
                TempData["CouponError"] = "Mã giảm giá không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (coupon.ExpiryDate < DateTime.Now)
            {
                TempData["CouponError"] = "Mã giảm giá đã hết hạn.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.Set(SessionKeys.Coupon, coupon);
            TempData["CouponMessage"] = $"Áp dụng mã {coupon.Code} thành công! Giảm {coupon.DiscountPercentage}%";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Cart/RemoveCoupon
        /// Gỡ bỏ mã giảm giá
        /// </summary>
        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.Remove(SessionKeys.Coupon);
            TempData["CouponMessage"] = "Đã gỡ bỏ mã giảm giá.";
            return RedirectToAction("Index");
        }
    }
}