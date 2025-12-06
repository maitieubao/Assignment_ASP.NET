using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Constants;
using Assignment_ASP.NET.Extensions;

namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = Roles.Customer)]
    public class MyOrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public MyOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// GET: /MyOrders
        /// Hiển thị danh sách đơn hàng của customer hiện tại
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(User.GetUserId());
            return View(orders);
        }

        /// <summary>
        /// GET: /MyOrders/Details/5
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value, includeDetails: true);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng");
            }

            // Kiểm tra quyền truy cập - chỉ cho phép xem đơn hàng của chính mình
            if (order.UserID != User.GetUserId())
            {
                return Forbid();
            }

            return View(order);
        }
    }
}
