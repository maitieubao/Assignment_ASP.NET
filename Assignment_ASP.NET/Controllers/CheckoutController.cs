using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Constants;
using Assignment_ASP.NET.Extensions;

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
                // Tạo đơn hàng thông qua OrderService
                var order = await _orderService.CreateOrderAsync(
                    User.GetUserId(),
                    cart,
                    shippingAddress,
                    paymentMethod
                );

                // Xóa giỏ hàng
                _cartService.ClearCart(HttpContext);

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
        /// GET: /Checkout/PaymentCallback
        /// Xử lý callback từ VNPAY
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Parse orderId safely
            if (!int.TryParse(response.OrderId, out int orderId))
            {
                 TempData["Error"] = "Lỗi xử lý đơn hàng từ VNPAY";
                 return RedirectToAction("Index", "Home");
            }

            if (response.Success || response.VnPayResponseCode == "00")
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed);
                TempData["PaymentSuccess"] = "Thanh toán VNPAY thành công!";
                return RedirectToAction("OrderConfirmation", new { orderId });
            }
            else
            {
                // Thanh toán thất bại
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Failed);
                TempData["Error"] = $"Lỗi thanh toán VNPAY: Mã lỗi {response.VnPayResponseCode}";
                return RedirectToAction("OrderConfirmation", new { orderId });
            }
        }

        /// <summary>
        /// GET: /Checkout/OrderConfirmation
        /// Hiển thị trang xác nhận đơn hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int? orderId)
        {
            Order? order;

            if (orderId == null)
            {
                // Lấy đơn hàng mới nhất của user
                var orders = await _orderService.GetOrdersByUserIdAsync(User.GetUserId());
                order = orders.FirstOrDefault();
            }
            else
            {
                order = await _orderService.GetOrderByIdAsync(orderId.Value, includeDetails: true);
            }

            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra quyền truy cập
            if (order.UserID != User.GetUserId())
            {
                return Forbid();
            }

            return View(order);
        }
    }
}