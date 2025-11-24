using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace Assignment_ASP.NET.Tests.Controllers
{
    [TestFixture]
    public class CheckoutControllerTests
    {
        private ApplicationDbContext _context;
        private CheckoutController _controller;
        private Mock<ISession> _mockSession;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Checkout_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed User
            _context.Users.Add(new User { UserID = 1, Username = "testuser", Email = "test@test.com", Address = "Test Address", RoleID = 3, FullName = "Test User", PasswordHash = "hash" });
            _context.SaveChanges();

            _mockSession = new Mock<ISession>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser"),
            }, "mock"));

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.Session = _mockSession.Object;

            _controller = new CheckoutController(_context);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithUser_WhenCartIsNotEmpty()
        {
            // Arrange
            var cart = new List<CartItem> { new CartItem { ProductID = 1, Quantity = 1, Price = 100 } };
            var serialized = JsonSerializer.Serialize(cart);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out bytes)).Returns(true);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<User>());
            var model = viewResult.Model as User;
            Assert.That(model.UserID, Is.EqualTo(1));
        }

        [Test]
        public async Task Index_RedirectsToCart_WhenCartIsEmpty()
        {
            // Arrange
            byte[] outBytes = null;
            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out outBytes)).Returns(false);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Cart"));
        }

        [Test]
        public async Task PlaceOrder_CreatesOrderAndDetails_AndClearsCart()
        {
            // Arrange
            var cart = new List<CartItem> { new CartItem { ProductID = 1, Quantity = 2, Price = 100 } };
            var serialized = JsonSerializer.Serialize(cart);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out bytes)).Returns(true);

            // Act
            var result = await _controller.PlaceOrder();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("OrderConfirmation"));

            // Verify Order created
            Assert.That(_context.Orders.Count(), Is.EqualTo(1));
            var order = await _context.Orders.FirstAsync();
            Assert.That(order.UserID, Is.EqualTo(1));
            Assert.That(order.TotalAmount, Is.EqualTo(200));

            // Verify OrderDetails created
            Assert.That(_context.OrderDetails.Count(), Is.EqualTo(1));
            var detail = await _context.OrderDetails.FirstAsync();
            Assert.That(detail.OrderID, Is.EqualTo(order.OrderID));
            Assert.That(detail.ProductID, Is.EqualTo(1));
            Assert.That(detail.Quantity, Is.EqualTo(2));

            // Verify Cart cleared
            _mockSession.Verify(s => s.Remove(CartController.CART_KEY), Times.Once);
        }

        [Test]
        public async Task PlaceOrder_RedirectsToCart_WhenCartIsEmpty()
        {
             // Arrange
            byte[] outBytes = null;
            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out outBytes)).Returns(false);

            // Act
            var result = await _controller.PlaceOrder();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Cart"));
        }
    }
}
