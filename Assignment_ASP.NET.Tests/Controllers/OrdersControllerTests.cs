using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_ASP.NET.Tests.Controllers
{
    [TestFixture]
    public class OrdersControllerTests
    {
        private ApplicationDbContext _context;
        private OrdersController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Orders_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed data
            var user = new User { UserID = 1, Username = "testuser", PasswordHash = "hash", FullName = "Test User", Email = "test@test.com", Address = "Address", Phone = "123", RoleID = 1 };
            _context.Users.Add(user);

            var order = new Order { OrderID = 1, UserID = 1, OrderDate = System.DateTime.Now, Status = "Pending", TotalAmount = 100, ShippingAddress = "Address" };
            _context.Orders.Add(order);

            var product = new Product { ProductID = 1, ProductName = "Product 1", Price = 100 };
            _context.Products.Add(product);

            _context.OrderDetails.Add(new OrderDetail { OrderDetailID = 1, OrderID = 1, ProductID = 1, Quantity = 1, Price = 100 });

            _context.SaveChanges();

            _controller = new OrdersController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithOrders()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<Order>;
            Assert.That(model.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Details_ReturnsViewResult_WithOrder()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Order;
            Assert.That(model.OrderID, Is.EqualTo(1));
            Assert.That(model.OrderDetails.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Edit_Post_UpdatesStatus()
        {
            // Arrange
            var order = await _context.Orders.FirstAsync();
            order.Status = "Shipped";

            // Act
            var result = await _controller.Edit(order.OrderID, order);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var updatedOrder = await _context.Orders.FindAsync(order.OrderID);
            Assert.That(updatedOrder.Status, Is.EqualTo("Shipped"));
        }

        [Test]
        public async Task DeleteConfirmed_RemovesOrderAndDetails()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_context.Orders.Count(), Is.EqualTo(0));
            Assert.That(_context.OrderDetails.Count(), Is.EqualTo(0));
        }
    }
}
