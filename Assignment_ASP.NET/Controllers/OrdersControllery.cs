using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Assignment_ASP.NET.Controllers
{
     [Authorize(Roles = "Admin,Employee")] // <-- BẠN NÊN THÊM NÀY SAU KHI LÀM XONG LOGIN
    // Chỉ Admin hoặc Employee mới được quản lý Đơn hàng
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Orders
        // Hiển thị danh sách tất cả đơn hàng
        public async Task<IActionResult> Index()
        {
            // Dùng Include() để lấy thông tin của User (người đặt hàng)
            // Sắp xếp theo ngày mới nhất lên trước
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Orders/Details/5
        // Hiển thị chi tiết một đơn hàng, bao gồm cả sản phẩm
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Đây là một query phức tạp, chúng ta cần:
            // 1. Lấy thông tin Order
            // 2. Lấy thông tin User (người đặt)
            // 3. Lấy danh sách OrderDetails (các món hàng)
            // 4. Với mỗi OrderDetail, lấy thông tin Product (tên, hình ảnh...)

            var order = await _context.Orders
                .Include(o => o.User) // Lấy User
                .Include(o => o.OrderDetails) // Lấy danh sách chi tiết
                    .ThenInclude(od => od.Product) // Lấy Product từ chi tiết
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: /Orders/Edit/5
        // Đây chính là chức năng "Duyệt đơn hàng"
        // Chỉ hiển thị form để cập nhật TRẠNG THÁI (Status)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Tạo một danh sách các trạng thái có thể chọn
            // Gán trạng thái hiện tại làm giá trị mặc định cho dropdown
            ViewBag.StatusList = new SelectList(
                new[] { "Pending", "Approved", "Shipped", "Canceled" },
                order.Status
            );

            return View(order);
        }

        // POST: /Orders/Edit/5
        // Xử lý cập nhật trạng thái
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,Status")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            // Chúng ta không dùng ModelState.IsValid vì chỉ binding 2 thuộc tính
            // Ta sẽ tìm đơn hàng gốc và chỉ cập nhật trạng thái
            var orderToUpdate = await _context.Orders.FindAsync(id);
            if (orderToUpdate == null)
            {
                return NotFound();
            }

            orderToUpdate.Status = order.Status; // Cập nhật trạng thái

            try
            {
                _context.Update(orderToUpdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.OrderID == order.OrderID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index)); // Quay về danh sách
        }

        // GET: /Orders/Delete/5
        // Hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Orders/Delete/5
        // Xử lý xóa (Lưu ý: Cần xóa OrderDetails trước)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // QUAN TRỌNG:
            // Phải xóa các bản ghi con (OrderDetails) trước khi xóa Order
            // để tránh lỗi Foreign Key Constraint.
            var orderDetails = await _context.OrderDetails
                                    .Where(od => od.OrderID == id)
                                    .ToListAsync();

            _context.OrderDetails.RemoveRange(orderDetails);

            // Giờ mới xóa Order cha
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}