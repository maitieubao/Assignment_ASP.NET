using Microsoft.AspNetCore.Mvc;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Helpers; // <-- Dùng helper chúng ta vừa tạo
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Authorization; // <-- Dùng model CartItem


namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Khóa (key) để lưu giỏ hàng trong Session
        public const string CART_KEY = "MyCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ Session
        private List<CartItem> GetCartItems()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
            if (cart == null)
            {
                cart = new List<CartItem>(); // Tạo mới nếu chưa có
            }
            return cart;
        }

        // Lưu giỏ hàng vào Session
        private void SaveCartToSession(List<CartItem> cart)
        {
            HttpContext.Session.Set(CART_KEY, cart);
        }

        // GET: /Cart
        // Hiển thị trang giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCartItems();

            // Tính tổng tiền
            ViewBag.TotalAmount = cart.Sum(item => item.Total);

            return View(cart); // Trả về View với model là List<CartItem>
        }

        // POST: /Cart/Add
        // Thêm sản phẩm vào giỏ (được gọi từ trang Home/Details)
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var cart = GetCartItems();

            // 1. Kiểm tra xem sản phẩm đã có trong giỏ chưa
            var existingItem = cart.FirstOrDefault(item => item.ProductID == productId);

            if (existingItem != null)
            {
                // 2a. Nếu đã có, chỉ tăng số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                // 2b. Nếu chưa có, lấy sản phẩm từ DB
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Sản phẩm không tồn tại");
                }

                // Tạo một CartItem mới
                var newItem = new CartItem(product)
                {
                    Quantity = quantity
                };
                cart.Add(newItem); // Thêm vào giỏ
            }

            // 3. Lưu giỏ hàng trở lại Session
            SaveCartToSession(cart);

            // 4. Chuyển hướng đến trang Giỏ hàng
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        // Xóa một sản phẩm khỏi giỏ
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCartItems();

            var itemToRemove = cart.FirstOrDefault(item => item.ProductID == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartToSession(cart); // Lưu lại
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/Update
        // Cập nhật số lượng
        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCartItems();
            var itemToUpdate = cart.FirstOrDefault(item => item.ProductID == productId);

            if (itemToUpdate != null)
            {
                if (quantity > 0)
                {
                    itemToUpdate.Quantity = quantity; // Cập nhật
                }
                else
                {
                    cart.Remove(itemToUpdate); // Xóa nếu số lượng = 0
                }
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        // Xóa toàn bộ giỏ hàng
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_KEY); // Xóa key khỏi Session
            return RedirectToAction("Index");
        }
    }
}