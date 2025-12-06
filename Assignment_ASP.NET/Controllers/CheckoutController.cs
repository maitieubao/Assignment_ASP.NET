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
        private readonly IZaloPayService _zaloPayService;
        private readonly IMoMoService _momoService;

        public CheckoutController(
            ApplicationDbContext context,
            IOrderService orderService,
            ICartService cartService,
            IVnPayService vnPayService,
            IZaloPayService zaloPayService,
            IMoMoService momoService)
        {
            _context = context;
            _orderService = orderService;
            _cartService = cartService;
            _vnPayService = vnPayService;
            _zaloPayService = zaloPayService;
            _momoService = momoService;
        }

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
                 paymentMethod != PaymentMethod.VnPay &&
                 paymentMethod != PaymentMethod.ZaloPay &&
                 paymentMethod != PaymentMethod.MoMo))
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
                // Tạo đơn hàng
                var order = await _orderService.CreateOrderAsync(
                    User.GetUserId(),
                    cart,
                    shippingAddress,
                    paymentMethod
                );

                // Xóa giỏ hàng
                _cartService.ClearCart(HttpContext);

                // Chuyển hướng dựa trên phương thức thanh toán
                string paymentUrl = paymentMethod switch
                {
                    PaymentMethod.VnPay => _vnPayService.CreatePaymentUrl(HttpContext, order),
                    PaymentMethod.ZaloPay => await _zaloPayService.CreatePaymentUrl(HttpContext, order),
                    PaymentMethod.MoMo => await _momoService.CreatePaymentUrl(HttpContext, order),
                    _ => Url.Action("OrderConfirmation", new { orderId = order.OrderID })
                };

                if (string.IsNullOrEmpty(paymentUrl))
                {
                    TempData["Error"] = "Không thể tạo URL thanh toán.";
                    return RedirectToAction("Index");
                }
                
                if (paymentMethod == PaymentMethod.COD)
                {
                    return Redirect(paymentUrl);
                }

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ZaloPayReturn()
        {
            var response = _zaloPayService.ProcessCallback(Request.Query);

            if (string.IsNullOrEmpty(response.OrderId) || !int.TryParse(response.OrderId, out int orderId))
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng";
                return RedirectToAction("Index", "Home");
            }

            if (response.Success)
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed);
                TempData["PaymentSuccess"] = "Thanh toán ZaloPay thành công!";
            }
            else
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Failed);
                TempData["Error"] = $"Thanh toán ZaloPay thất bại. Mã lỗi: {response.Status}";
            }

            return RedirectToAction("OrderConfirmation", new { orderId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> MoMoReturn()
        {
            var response = _momoService.ProcessCallback(Request.Query);

            if (!int.TryParse(response.OrderId, out int orderId))
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng";
                return RedirectToAction("Index", "Home");
            }

            if (response.Success)
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed);
                TempData["PaymentSuccess"] = "Thanh toán MoMo thành công!";
            }
            else
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Failed);
                TempData["Error"] = $"Thanh toán MoMo thất bại. Mã lỗi: {response.ResultCode}";
            }

            return RedirectToAction("OrderConfirmation", new { orderId });
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int? orderId)
        {
            Order? order;

            if (orderId == null)
            {
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

            if (order.UserID != User.GetUserId())
            {
                return Forbid();
            }

            return View(order);
        }
    }
}