using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Authorization; // Sẽ cần khi bạn làm Login

namespace Assignment_ASP.NET.Controllers
{
     [Authorize(Roles = "Admin")] // <-- BẮT BUỘC SAU KHI LÀM LOGIN
    // Chỉ Admin mới được quản lý Vai trò
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Roles
        // Hiển thị danh sách tất cả vai trò
        public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.ToListAsync());
        }

        // GET: /Roles/Details/5
        // Hiển thị chi tiết (dù chỉ có 1 trường tên)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleID == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // GET: /Roles/Create
        // Hiển thị form tạo mới
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleName")] Role role)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem RoleName đã tồn tại chưa
                var existingRole = await _context.Roles
                                         .FirstOrDefaultAsync(r => r.RoleName == role.RoleName);
                if (existingRole != null)
                {
                    ModelState.AddModelError("RoleName", "Tên vai trò này đã tồn tại.");
                    return View(role);
                }

                _context.Add(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: /Roles/Edit/5
        // Hiển thị form chỉnh sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleID,RoleName")] Role role)
        {
            if (id != role.RoleID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên mới có trùng với tên khác không
                var existingRole = await _context.Roles
                                         .FirstOrDefaultAsync(r => r.RoleName == role.RoleName && r.RoleID != role.RoleID);
                if (existingRole != null)
                {
                    ModelState.AddModelError("RoleName", "Tên vai trò này đã tồn tại.");
                    return View(role);
                }

                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Roles.Any(e => e.RoleID == role.RoleID))
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
            return View(role);
        }

        // GET: /Roles/Delete/5
        // Hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleID == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: /Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                // LƯU Ý: Cần kiểm tra xem có User nào đang dùng Role này không
                // trước khi xóa để tránh lỗi Foreign Key
                var userWithThisRole = await _context.Users.AnyAsync(u => u.RoleID == id);

                if (userWithThisRole)
                {
                    // Nếu có User đang dùng, báo lỗi, không cho xóa
                    ModelState.AddModelError(string.Empty, "Không thể xóa vai trò này vì đang có người dùng sử dụng.");
                    return View("Delete", role); // Trả về view Delete với thông báo lỗi
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}