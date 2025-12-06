using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Constants;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Assignment_ASP.NET.Services
{
    public interface IAccountService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<(bool Success, string ErrorMessage)> RegisterAsync(User user, string password, string confirmPassword);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateProfileAsync(string username, User model, string? newPassword);
        ClaimsPrincipal CreateClaimsPrincipal(User user);
    }

    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var hashedPassword = HashPassword(password);
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(User user, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                return (false, "Mật khẩu không khớp");
            }

            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return (false, "Tên đăng nhập đã tồn tại");
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return (false, "Email đã tồn tại");
            }

            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == Roles.Customer);
            if (customerRole == null)
            {
                return (false, "Lỗi hệ thống: Không tìm thấy role Customer");
            }

            user.RoleID = customerRole.RoleID;
            user.PasswordHash = HashPassword(password);

            _context.Add(user);
            await _context.SaveChangesAsync();

            // Load role for the newly created user to allow immediate login
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return (true, string.Empty);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UpdateProfileAsync(string username, User model, string? newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return false;
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = HashPassword(newPassword);
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public ClaimsPrincipal CreateClaimsPrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
