using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Constants;
using Assignment_ASP.NET.Extensions;
using Assignment_ASP.NET.Helpers;

namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = Roles.Customer)]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IVnPayService _vnPayService;

        public CheckoutController(
            ApplicationDbContext context,
            IOrderService orderService,
            ICartService cartService,
            IVnPayService vnPayService)
        {
            _context = context;
            _orderService = orderService;
            _cartService = cartService;
            _vnPayService = vnPayService;
        }

        /// <summary>
        /// GET: /Checkout/Index
        /// Hiển thị trang checkout với thông tin đơn hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCartItems(HttpContext);
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var userId = User.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng");
            }

            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = _cartService.GetCartTotal(HttpContext);

            return View(user);
        }

        /// <summary>
        /// POST: /Checkout/PlaceOrder
        /// Xử lý đặt hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string paymentMethod, string shippingAddress)
        {
            var cart = _cartService.GetCartItems(HttpContext);
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // Validate payment method
            if (string.IsNullOrEmpty(paymentMethod) ||
                (paymentMethod != PaymentMethod.COD && 
                 paymentMethod != PaymentMethod.Bank &&
                 paymentMethod != PaymentMethod.VnPay))
            {
                TempData["Error"] = "Vui lòng chọn phương thức thanh toán";
                return RedirectToAction("Index");
            }

            // Validate shipping address
            if (string.IsNullOrEmpty(shippingAddress))
            {
                var userId = User.GetUserId();
                var user = await _context.Users.FindAsync(userId);
                shippingAddress = user?.Address ?? "";
            }

            try
            {
                // Lấy mã giảm giá từ session (nếu có)
                var coupon = HttpContext.Session.Get<Coupon>(SessionKeys.Coupon);

                // Tạo đơn hàng thông qua OrderService (với coupon)
                var order = await _orderService.CreateOrderAsync(
                    User.GetUserId(),
                    cart,
                    shippingAddress,
                    paymentMethod,
                    coupon
                );

                // Xóa giỏ hàng và mã giảm giá
                _cartService.ClearCart(HttpContext);
                HttpContext.Session.Remove(SessionKeys.Coupon);

                // Chuyển hướng dựa trên phương thức thanh toán
                if (paymentMethod == PaymentMethod.VnPay)
                {
                    var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, order);
                    return Redirect(paymentUrl);
                }
                else if (paymentMethod == PaymentMethod.Bank)
                {
                    return RedirectToAction("BankPayment", new { orderId = order.OrderID });
                }
                else
                {
                    return RedirectToAction("OrderConfirmation", new { orderId = order.OrderID });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// GET: /Checkout/BankPayment
        /// Hiển thị trang thanh toán ngân hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BankPayment(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId, includeDetails: true);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng");
            }

            // Kiểm tra quyền truy cập
            if (order.UserID != User.GetUserId())
            {
                return Forbid();
            }

            return View(order);
        }

        /// <summary>
        /// POST: /Checkout/ProcessBankPayment
        /// Xử lý thanh toán ngân hàng (giả lập)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBankPayment(int orderId, string bankCode)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng");
            }

            // Kiểm tra quyền truy cập
            if (order.UserID != User.GetUserId())
            {
                return Forbid();
            }

            // Validate bank code
            if (string.IsNullOrEmpty(bankCode) || !BankCodes.AllBanks.Contains(bankCode))
            {
                TempData["Error"] = "Vui lòng chọn ngân hàng";
                return RedirectToAction("BankPayment", new { orderId });
            }

            // Cập nhật trạng thái thanh toán
            await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed);

            TempData["PaymentSuccess"] = $"Thanh toán qua {bankCode} thành công!";
            return RedirectToAction("OrderConfirmation", new { orderId });
        }

        /// <summary>
        /// GET: /Checkout/VnPayReturn
        /// Xử lý callback từ VNPAY
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            var response = _vnPayService.ProcessCallback(Request.Query);

            if (!int.TryParse(response.OrderId, out int orderId))
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng";
                return RedirectToAction("Index", "Home");
            }

            if (response.Success)
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed);
                TempData["PaymentSuccess"] = "Thanh toán VNPay thành công!";
            }
            else
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Failed);
                TempData["Error"] = $"Thanh toán thất bại. Mã lỗi: {response.ResponseCode}";
            }

            return RedirectToAction("OrderConfirmation", new { orderId });
        }

        /// <summary>
        /// GET: /Checkout/OrderConfirmation
        /// Hiển thị trang xác nhận đơn hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int? orderId)
        {
            Order? order;
            
            try 
            {
                var userId = User.GetUserId();

                if (orderId == null)
                {
                    // Lấy đơn hàng mới nhất của user
                    var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                    order = orders.FirstOrDefault();
                }
                else
                {
                    order = await _orderService.GetOrderByIdAsync(orderId.Value, includeDetails: true);
                }

                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra quyền truy cập (Cho phép Admin/Employee xem luôn)
                if (order.UserID != userId && !User.IsInRole(Roles.Admin) && !User.IsInRole(Roles.Employee))
                {
                    TempData["Error"] = "Bạn không có quyền xem đơn hàng này.";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                 TempData["Error"] = $"Lỗi khi tải đơn hàng: {ex.Message}";
                 return RedirectToAction("Index", "Home");
            }
        }
    }
}