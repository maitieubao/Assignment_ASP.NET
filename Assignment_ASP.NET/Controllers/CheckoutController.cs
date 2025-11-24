using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Cần cho [Authorize]
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Helpers; // Cần cho SessionExtensions
using System.Security.Claims; // Cần để lấy UserID

namespace Assignment_ASP.NET.Controllers
{
    // BẮT BUỘC: Chỉ người đã đăng nhập mới được vào trang này
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ Session
        private List<CartItem> GetCartItems()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CartController.CART_KEY);
            return cart ?? new List<CartItem>();
        }

        // Lấy UserID của người đang đăng nhập
        private int GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier là ID của User (đã được lưu khi Login)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new Exception("Không thể xác định người dùng.");
        }

        // GET: /Checkout/Index
        // Hiển thị trang xác nhận thông tin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = GetCartItems();
            if (cart.Count == 0)
            {
                // Giỏ hàng trống, quay về trang giỏ hàng
                return RedirectToAction("Index", "Cart");
            }

            // Lấy thông tin người dùng hiện tại
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng");
            }

            // Gửi thông tin giỏ hàng và tổng tiền qua ViewBag
            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = cart.Sum(item => item.Total);

            // Gửi thông tin user (như địa chỉ) làm Model
            return View(user);
        }

        // POST: /Checkout/PlaceOrder
        // Xử lý đặt hàng (chuyển từ Session -> Database)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = GetCartItems();
            var userId = GetCurrentUserId();

            if (cart.Count == 0)
            {
                // Lỗi: Giỏ hàng trống
                return RedirectToAction("Index", "Cart");
            }

            // Lấy thông tin người dùng (để lấy địa chỉ)
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng");
            }

            // 1. TẠO ĐƠN HÀNG (ORDER)
            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(item => item.Total),
                Status = "Pending", // Trạng thái ban đầu
                ShippingAddress = user.Address // Lấy địa chỉ mặc định của user
            };

            // 2. THÊM ORDER VÀO DATABASE (Lưu ý: Phải Save 1 lần để lấy OrderID)
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Lấy được OrderID tự tăng (sau khi đã Save)
            int newOrderId = order.OrderID;

            // 3. TẠO CHI TIẾT ĐƠN HÀNG (ORDER DETAILS)
            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = newOrderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Price // Lưu lại giá tại thời điểm mua
                };
                _context.OrderDetails.Add(orderDetail);
            }

            // 4. LƯU TẤT CẢ CHI TIẾT ĐƠN HÀNG VÀO DATABASE
            await _context.SaveChangesAsync();

            // 5. XÓA GIỎ HÀNG KHỎI SESSION
            HttpContext.Session.Remove(CartController.CART_KEY);

            // 6. CHUYỂN HƯỚNG TỚI TRANG CẢM ƠN
            return RedirectToAction("OrderConfirmation");
        }

        // GET: /Checkout/OrderConfirmation
        // Trang cảm ơn sau khi đặt hàng
        [HttpGet]
        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}