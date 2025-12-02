using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho OrdersController
    /// </summary>
    [TestFixture]
    public class OrdersControllerTests : ControllerTestBase
    {
        private OrdersController _controller = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Orders";

        protected override void SeedCommonData()
        {
            // Seed user
            SeedDefaultUser();

            // Seed product
            Context.Products.Add(TestDataBuilder.CreateProduct(
                TestConstants.IPhone14ProductId,
                TestConstants.IPhone14ProductName,
                TestConstants.PhoneCategoryId,
                TestConstants.IPhone14Price
            ));

            // Seed order
            var order = new Order 
            { 
                OrderID = 1, 
                UserID = TestConstants.DefaultUserId, 
                OrderDate = System.DateTime.Now, 
                Status = "Pending", 
                TotalAmount = TestConstants.IPhone14Price, 
                ShippingAddress = TestConstants.DefaultAddress 
            };
            Context.Orders.Add(order);

            // Seed order detail
            Context.OrderDetails.Add(new OrderDetail 
            { 
                OrderDetailID = 1, 
                OrderID = 1, 
                ProductID = TestConstants.IPhone14ProductId, 
                Quantity = 1, 
                Price = TestConstants.IPhone14Price 
            });

            Context.SaveChanges();
        }

        protected override void AdditionalSetup()
        {
            _controller = new OrdersController(Context);
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }

        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithOrders()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as List<Order>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(1), "Should return 1 order");
        }

        #endregion

        #region Details Tests

        [Test]
        public async Task Details_ReturnsViewResult_WithOrder()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as Order;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.OrderID, Is.EqualTo(1));
            Assert.That(model.OrderDetails.Count, Is.EqualTo(1), "Order should have 1 order detail");
        }

        #endregion

        #region Edit Tests

        [Test]
        public async Task Edit_Post_UpdatesStatus()
        {
            // Arrange
            var order = Context.Orders.First();
            var originalStatus = order.Status;
            order.Status = "Shipped";

            // Act
            var result = await _controller.Edit(order.OrderID, order);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var updatedOrder = await Context.Orders.FindAsync(order.OrderID);
            Assert.That(updatedOrder, Is.Not.Null);
            Assert.That(updatedOrder!.Status, Is.EqualTo("Shipped"));
            Assert.That(updatedOrder.Status, Is.Not.EqualTo(originalStatus), "Status should be updated");
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task DeleteConfirmed_RemovesOrderAndDetails()
        {
            // Arrange
            var initialOrderCount = Context.Orders.Count();
            var initialDetailCount = Context.OrderDetails.Count();

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(Context.Orders.Count(), Is.EqualTo(initialOrderCount - 1), "Should remove order");
            Assert.That(Context.OrderDetails.Count(), Is.EqualTo(initialDetailCount - 1), "Should remove order details");
        }

        #endregion
    }
}
