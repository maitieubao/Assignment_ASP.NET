using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using System.Text.Json;

namespace Assignment_ASP.NET.Tests.Helpers
{
    /// <summary>
    /// Helper methods để làm việc với Session trong tests
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Setup mock session với cart rỗng
        /// </summary>
        public static void SetupEmptyCart(Mock<ISession> mockSession)
        {
            byte[] outBytes = null;
            mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out outBytes))
                .Returns(false);
        }

        /// <summary>
        /// Setup mock session với cart items
        /// </summary>
        public static void SetupCartWithItems(Mock<ISession> mockSession, List<CartItem> cartItems)
        {
            var serialized = JsonSerializer.Serialize(cartItems);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out bytes))
                .Returns(true);
        }

        /// <summary>
        /// Verify rằng cart đã được set vào session
        /// </summary>
        public static void VerifyCartSet(Mock<ISession> mockSession, Times times)
        {
            mockSession.Verify(s => s.Set(CartController.CART_KEY, It.IsAny<byte[]>()), times);
        }

        /// <summary>
        /// Verify rằng cart đã được remove khỏi session
        /// </summary>
        public static void VerifyCartRemoved(Mock<ISession> mockSession, Times times)
        {
            mockSession.Verify(s => s.Remove(CartController.CART_KEY), times);
        }

        /// <summary>
        /// Verify cart được set với số lượng items cụ thể
        /// </summary>
        public static void VerifyCartSetWithItemCount(Mock<ISession> mockSession, int expectedCount)
        {
            mockSession.Verify(s => s.Set(CartController.CART_KEY, It.Is<byte[]>(b =>
                JsonSerializer.Deserialize<List<CartItem>>(Encoding.UTF8.GetString(b), (JsonSerializerOptions)null).Count == expectedCount
            )), Times.Once);
        }

        /// <summary>
        /// Verify cart được set với quantity cụ thể cho product
        /// </summary>
        public static void VerifyCartSetWithQuantity(Mock<ISession> mockSession, int productId, int expectedQuantity)
        {
            mockSession.Verify(s => s.Set(CartController.CART_KEY, It.Is<byte[]>(b =>
                JsonSerializer.Deserialize<List<CartItem>>(Encoding.UTF8.GetString(b), (JsonSerializerOptions)null)
                    .First(item => item.ProductID == productId).Quantity == expectedQuantity
            )), Times.Once);
        }
    }
}
