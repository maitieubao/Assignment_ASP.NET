using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Constants;
using Assignment_ASP.NET.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Assignment_ASP.NET.Services
{
    /// <summary>
    /// Interface cho Cart Service
    /// </summary>
    public interface ICartService
    {
        List<CartItem> GetCartItems(HttpContext httpContext);
        void SaveCart(HttpContext httpContext, List<CartItem> cart);
        Task<bool> AddToCartAsync(HttpContext httpContext, int productId, int quantity = 1);
        bool RemoveFromCart(HttpContext httpContext, int productId);
        bool UpdateQuantity(HttpContext httpContext, int productId, int quantity);
        void ClearCart(HttpContext httpContext);
        decimal GetCartTotal(HttpContext httpContext);
        int GetCartItemCount(HttpContext httpContext);
    }

    /// <summary>
    /// Service xử lý logic nghiệp vụ liên quan đến giỏ hàng
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy giỏ hàng từ Session
        /// </summary>
        public List<CartItem> GetCartItems(HttpContext httpContext)
        {
            var cart = httpContext.Session.Get<List<CartItem>>(SessionKeys.Cart);
            return cart ?? new List<CartItem>();
        }

        /// <summary>
        /// Lưu giỏ hàng vào Session
        /// </summary>
        public void SaveCart(HttpContext httpContext, List<CartItem> cart)
        {
            httpContext.Session.Set(SessionKeys.Cart, cart);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        public async Task<bool> AddToCartAsync(HttpContext httpContext, int productId, int quantity = 1)
        {
            var cart = GetCartItems(httpContext);

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var existingItem = cart.FirstOrDefault(item => item.ProductID == productId);

            if (existingItem != null)
            {
                // Tăng số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                // Lấy sản phẩm từ database
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // Tạo CartItem mới
                var newItem = new CartItem(product)
                {
                    Quantity = quantity
                };
                cart.Add(newItem);
            }

            SaveCart(httpContext, cart);
            return true;
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        public bool RemoveFromCart(HttpContext httpContext, int productId)
        {
            var cart = GetCartItems(httpContext);
            var itemToRemove = cart.FirstOrDefault(item => item.ProductID == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCart(httpContext, cart);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm
        /// </summary>
        public bool UpdateQuantity(HttpContext httpContext, int productId, int quantity)
        {
            var cart = GetCartItems(httpContext);
            var itemToUpdate = cart.FirstOrDefault(item => item.ProductID == productId);

            if (itemToUpdate != null)
            {
                if (quantity > 0)
                {
                    itemToUpdate.Quantity = quantity;
                }
                else
                {
                    cart.Remove(itemToUpdate);
                }

                SaveCart(httpContext, cart);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public void ClearCart(HttpContext httpContext)
        {
            httpContext.Session.Remove(SessionKeys.Cart);
        }

        /// <summary>
        /// Tính tổng tiền giỏ hàng
        /// </summary>
        public decimal GetCartTotal(HttpContext httpContext)
        {
            var cart = GetCartItems(httpContext);
            return cart.Sum(item => item.Total);
        }

        /// <summary>
        /// Đếm số lượng items trong giỏ
        /// </summary>
        public int GetCartItemCount(HttpContext httpContext)
        {
            var cart = GetCartItems(httpContext);
            return cart.Sum(item => item.Quantity);
        }
    }
}
