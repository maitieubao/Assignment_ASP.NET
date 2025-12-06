using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Constants;
using Microsoft.EntityFrameworkCore;

namespace Assignment_ASP.NET.Services
{
    /// <summary>
    /// Interface cho Order Service
    /// </summary>
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int userId, List<CartItem> cartItems, string shippingAddress, string paymentMethod);
        Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus);
        Task<bool> UpdateOrderStatusAsync(int orderId, string orderStatus);
        Task<Order?> GetOrderByIdAsync(int orderId, bool includeDetails = false);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<bool> DeleteOrderAsync(int orderId);
    }

    /// <summary>
    /// Service xử lý logic nghiệp vụ liên quan đến đơn hàng
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo đơn hàng mới từ giỏ hàng
        /// </summary>
        public async Task<Order> CreateOrderAsync(int userId, List<CartItem> cartItems, string shippingAddress, string paymentMethod)
        {
            if (cartItems == null || !cartItems.Any())
            {
                throw new ArgumentException("Giỏ hàng trống");
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(item => item.Total),
                Status = OrderStatus.Pending,
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            return order;
        }

        /// <summary>
        /// Cập nhật trạng thái thanh toán
        /// </summary>
        public async Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            order.PaymentStatus = paymentStatus;

            // Nếu thanh toán thành công qua các cổng thanh toán online, tự động duyệt đơn
            if (paymentStatus == PaymentStatus.Completed &&
                (order.PaymentMethod == PaymentMethod.VnPay ||
                 order.PaymentMethod == PaymentMethod.ZaloPay ||
                 order.PaymentMethod == PaymentMethod.MoMo))
            {
                order.Status = OrderStatus.Approved;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string orderStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            order.Status = orderStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy đơn hàng theo ID
        /// </summary>
        public async Task<Order?> GetOrderByIdAsync(int orderId, bool includeDetails = false)
        {
            var query = _context.Orders.AsQueryable();

            if (includeDetails)
            {
                query = query
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product);
            }

            return await query.FirstOrDefaultAsync(o => o.OrderID == orderId);
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của user
        /// </summary>
        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả đơn hàng (cho Admin/Employee)
        /// </summary>
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// Xóa đơn hàng và chi tiết đơn hàng
        /// </summary>
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderID == orderId)
                .ToListAsync();

            _context.OrderDetails.RemoveRange(orderDetails);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
