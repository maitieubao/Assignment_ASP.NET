using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho CheckoutController
    /// </summary>
    [TestFixture]
    public class CheckoutControllerTests : ControllerTestBase
    {
        private CheckoutController _controller = null!;
        private Mock<ISession> _mockSession = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Checkout";

        protected override void SeedCommonData()
        {
            // Seed user cho checkout tests
            SeedDefaultUser();
        }

        protected override void AdditionalSetup()
        {
            _mockSession = new Mock<ISession>();

            // Setup authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestConstants.DefaultUserId.ToString()),
                new Claim(ClaimTypes.Name, TestConstants.DefaultUsername),
            }, "mock"));

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.Session = _mockSession.Object;

            _controller = new CheckoutController(Context);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }

        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithUser_WhenCartIsNotEmpty()
        {
            // Arrange
            var cart = TestDataBuilder.CreateDefaultCartItems();
            SessionHelper.SetupCartWithItems(_mockSession, cart);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<User>());
            var model = viewResult.Model as User;
            Assert.That(model.UserID, Is.EqualTo(TestConstants.DefaultUserId));
        }

        [Test]
        public async Task Index_RedirectsToCart_WhenCartIsEmpty()
        {
            // Arrange
            SessionHelper.SetupEmptyCart(_mockSession);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo(TestConstants.IndexAction));
            Assert.That(redirectResult.ControllerName, Is.EqualTo(TestConstants.CartController));
        }

        #endregion

        #region PlaceOrder Tests

        [Test]
        public async Task PlaceOrder_CreatesOrderAndDetails_AndClearsCart()
        {
            // Arrange
            var cart = new List<CartItem>
            {
                TestDataBuilder.CreateCartItem(TestConstants.IPhone14ProductId, 2, TestConstants.IPhone14Price)
            };
            SessionHelper.SetupCartWithItems(_mockSession, cart);

            // Act
            var result = await _controller.PlaceOrder();

            // Assert - Verify redirect
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("OrderConfirmation"));

            // Verify Order created
            Assert.That(Context.Orders.Count(), Is.EqualTo(1), "Should create one order");
            var order = Context.Orders.First();
            Assert.That(order.UserID, Is.EqualTo(TestConstants.DefaultUserId));
            Assert.That(order.TotalAmount, Is.EqualTo(2 * TestConstants.IPhone14Price), "Total should be quantity * price");

            // Verify OrderDetails created
            Assert.That(Context.OrderDetails.Count(), Is.EqualTo(1), "Should create one order detail");
            var detail = Context.OrderDetails.First();
            Assert.That(detail.OrderID, Is.EqualTo(order.OrderID));
            Assert.That(detail.ProductID, Is.EqualTo(TestConstants.IPhone14ProductId));
            Assert.That(detail.Quantity, Is.EqualTo(2));

            // Verify Cart cleared
            SessionHelper.VerifyCartRemoved(_mockSession, Times.Once());
        }

        [Test]
        public async Task PlaceOrder_RedirectsToCart_WhenCartIsEmpty()
        {
            // Arrange
            SessionHelper.SetupEmptyCart(_mockSession);

            // Act
            var result = await _controller.PlaceOrder();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo(TestConstants.IndexAction));
            Assert.That(redirectResult.ControllerName, Is.EqualTo(TestConstants.CartController));
            
            // Verify no order was created
            Assert.That(Context.Orders.Count(), Is.EqualTo(0), "Should not create order when cart is empty");
        }

        #endregion
    }
}
