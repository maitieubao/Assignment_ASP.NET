using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Authorization;

namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.Users)
                .ToListAsync();
            return View(roles);
        }

        // GET: /Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(m => m.RoleID == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // GET: /Roles/Create
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .Include(r => r.Users)
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
                var userWithThisRole = await _context.Users.AnyAsync(u => u.RoleID == id);

                if (userWithThisRole)
                {
                    ModelState.AddModelError(string.Empty, "Không thể xóa vai trò này vì đang có người dùng sử dụng.");
                    return View("Delete", role);
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}