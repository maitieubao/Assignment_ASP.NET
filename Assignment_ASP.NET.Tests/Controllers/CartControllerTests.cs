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
using System.Text;
using System.Text.Json;

namespace Assignment_ASP.NET.Tests.Controllers
{
    [TestFixture]
    public class CartControllerTests
    {
        private ApplicationDbContext _context;
        private CartController _controller;
        private Mock<ISession> _mockSession;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Cart_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed data
            _context.Products.Add(new Product { ProductID = 1, ProductName = "iPhone 14", Price = 1000, ImageUrl = "img.jpg" });
            _context.SaveChanges();

            _mockSession = new Mock<ISession>();
            
            var httpContext = new DefaultHttpContext();
            httpContext.Session = _mockSession.Object;

            _controller = new CartController(_context);
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
        public async Task Add_AddsNewItemToCart_WhenCartIsEmpty()
        {
            // Arrange
            byte[] outBytes = null;
            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out outBytes)).Returns(false);

            // Act
            var result = await _controller.Add(1, 1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            
            // Verify Session.Set was called
            _mockSession.Verify(s => s.Set(CartController.CART_KEY, It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task Add_IncrementsQuantity_WhenItemExists()
        {
            // Arrange
            var existingCart = new List<CartItem>
            {
                new CartItem { ProductID = 1, Quantity = 1, Price = 1000 }
            };
            var serialized = JsonSerializer.Serialize(existingCart);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out bytes)).Returns(true);

            // Act
            await _controller.Add(1, 1);

            // Assert
            // Verify Set is called with updated quantity (2)
            _mockSession.Verify(s => s.Set(CartController.CART_KEY, It.Is<byte[]>(b => 
                JsonSerializer.Deserialize<List<CartItem>>(Encoding.UTF8.GetString(b), (JsonSerializerOptions)null).First().Quantity == 2
            )), Times.Once);
        }

        [Test]
        public void Remove_RemovesItemFromCart()
        {
            // Arrange
            var existingCart = new List<CartItem>
            {
                new CartItem { ProductID = 1, Quantity = 1, Price = 1000 }
            };
            var serialized = JsonSerializer.Serialize(existingCart);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out bytes)).Returns(true);

            // Act
            var result = _controller.Remove(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            
            // Verify Set is called with empty list or list without item
            _mockSession.Verify(s => s.Set(CartController.CART_KEY, It.Is<byte[]>(b => 
                JsonSerializer.Deserialize<List<CartItem>>(Encoding.UTF8.GetString(b), (JsonSerializerOptions)null).Count == 0
            )), Times.Once);
        }

        [Test]
        public void Update_UpdatesQuantity()
        {
            // Arrange
            var existingCart = new List<CartItem>
            {
                new CartItem { ProductID = 1, Quantity = 1, Price = 1000 }
            };
            var serialized = JsonSerializer.Serialize(existingCart);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            _mockSession.Setup(s => s.TryGetValue(CartController.CART_KEY, out bytes)).Returns(true);

            // Act
            var result = _controller.Update(1, 5);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            
            _mockSession.Verify(s => s.Set(CartController.CART_KEY, It.Is<byte[]>(b => 
                JsonSerializer.Deserialize<List<CartItem>>(Encoding.UTF8.GetString(b), (JsonSerializerOptions)null).First().Quantity == 5
            )), Times.Once);
        }

        [Test]
        public void Clear_RemovesCartFromSession()
        {
            // Act
            var result = _controller.Clear();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            _mockSession.Verify(s => s.Remove(CartController.CART_KEY), Times.Once);
        }
    }
}
