using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using System.Security.Cryptography; // Cần cho băm mật khẩu
using System.Text;
using Microsoft.AspNetCore.Authorization; // Cần cho băm mật khẩu

namespace Assignment_ASP.NET.Controllers
{
     [Authorize(Roles = "Admin")] // <-- BẮT BUỘC SAU KHI LÀM LOGIN
    // Chỉ Admin mới được quản lý Người dùng
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Users
        // Hiển thị danh sách người dùng
        public async Task<IActionResult> Index()
        {
            // Dùng Include() để lấy thông tin Role đi kèm
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        // GET: /Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role) // Lấy thông tin Role
                .FirstOrDefaultAsync(m => m.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: /Users/Create
        // Hiển thị form tạo mới
        public IActionResult Create()
        {
            // Load danh sách Role cho dropdown
            PopulateRolesDropDownList();
            return View();
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Username,FullName,Email,Address,Phone,RoleID")] User user,
            string password) // Nhận mật khẩu thô từ form
        {
            // Chúng ta không dùng [Bind("PasswordHash")] vì cần xử lý
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "Mật khẩu là bắt buộc.");
            }

            // Kiểm tra Username và Email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            }
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Băm mật khẩu
                user.PasswordHash = HashPassword(password);

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại dropdown
            PopulateRolesDropDownList(user.RoleID);
            return View(user);
        }

        // GET: /Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Load dropdown và chọn sẵn Role hiện tại
            PopulateRolesDropDownList(user.RoleID);
            return View(user);
        }

        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("UserID,Username,FullName,Email,Address,Phone,RoleID")] User user,
            string? password) // Mật khẩu có thể null (không bắt buộc)
        {
            if (id != user.UserID)
            {
                return NotFound();
            }

            // Lấy thông tin User gốc từ DB
            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            // Kiểm tra Username/Email trùng lặp (trừ chính nó)
            if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.UserID != id))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            }
            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserID != id))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Cập nhật các trường
                userToUpdate.Username = user.Username;
                userToUpdate.FullName = user.FullName;
                userToUpdate.Email = user.Email;
                userToUpdate.Address = user.Address;
                userToUpdate.Phone = user.Phone;
                userToUpdate.RoleID = user.RoleID;

                // CHỈ cập nhật mật khẩu nếu nó được cung cấp
                if (!string.IsNullOrEmpty(password))
                {
                    userToUpdate.PasswordHash = HashPassword(password);
                }

                // Mật khẩu hash gốc được giữ nguyên nếu password là null

                try
                {
                    _context.Update(userToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserID == user.UserID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại dropdown
            PopulateRolesDropDownList(user.RoleID);
            return View(user);
        }

        // GET: /Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // CẢNH BÁO: Bạn cần kiểm tra xem User này có "Orders" không
            var hasOrders = await _context.Orders.AnyAsync(o => o.UserID == id);
            if (hasOrders)
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa người dùng này vì họ đã có đơn hàng. Hãy cân nhắc vô hiệu hóa tài khoản.");
                return View("Delete", user);
            }

            // (Tùy chọn) Ngăn Admin tự xóa chính mình
            // if (user.Username == User.Identity.Name)
            // {
            //     ModelState.AddModelError(string.Empty, "Bạn không thể tự xóa tài khoản của mình.");
            //     return View("Delete", user);
            // }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // --- HÀM HỖ TRỢ (HELPER METHODS) ---

        // Hàm hỗ trợ load danh sách Role cho Dropdown
        private void PopulateRolesDropDownList(object selectedRole = null)
        {
            var rolesQuery = from r in _context.Roles
                             orderby r.RoleName
                             select r;

            ViewBag.RoleID = new SelectList(rolesQuery.AsNoTracking(),
                                                "RoleID", "RoleName",
                                                selectedRole);
        }

        // Hàm băm mật khẩu (Dùng SHA256 cho nhất quán với Seed Data)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // Chuyển về chuỗi hex
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}