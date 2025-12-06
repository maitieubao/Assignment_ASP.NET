using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Constants;

namespace Assignment_ASP.NET.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            var user = await _accountService.AuthenticateAsync(username, password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            var principal = _accountService.CreateClaimsPrincipal(user);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role.RoleName == Roles.Admin || user.Role.RoleName == Roles.Employee)
            {
                return RedirectToAction("Index", "Products");
            }

            return RedirectToAction("Index", "Home");
        }

        // LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // REGISTER
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            [Bind("Username,FullName,Email,Address,Phone")] User user,
            string password, string confirmPassword)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var (success, errorMessage) = await _accountService.RegisterAsync(user, password, confirmPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(user);
            }

            // Auto login after register
            var principal = _accountService.CreateClaimsPrincipal(user);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        // ACCESS DENIED
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // USER PROFILE
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity!.Name;
            if (username == null) return NotFound();

            var user = await _accountService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var username = User.Identity!.Name;
            if (username == null) return NotFound();

            var user = await _accountService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(User model, string? newPassword)
        {
            var username = User.Identity!.Name;
            if (username == null) return NotFound();

            var success = await _accountService.UpdateProfileAsync(username, model, newPassword);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật thành công!";
            return RedirectToAction("Profile");
        }

        /// <summary>
        /// GET: /Account/OrderHistory
        /// Redirect đến MyOrders controller
        /// </summary>
        [HttpGet]
        public IActionResult OrderHistory()
        {
            return RedirectToAction("Index", "MyOrders");
        }
    }
}