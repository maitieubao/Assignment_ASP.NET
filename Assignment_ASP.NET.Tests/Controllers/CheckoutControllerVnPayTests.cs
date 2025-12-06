using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho CheckoutController với VNPay integration
    /// </summary>
    [TestFixture]
    public class CheckoutControllerVnPayTests : ControllerTestBase
    {
        private CheckoutController _controller = null!;
        private Mock<IVnPayService> _mockVnPayService = null!;
        private Mock<IOrderService> _mockOrderService = null!;
        private Mock<ICartService> _mockCartService = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_CheckoutVnPay";

        protected override void SeedCommonData()
        {
            SeedDefaultUser();
            SeedDefaultProducts();
        }

        protected override void AdditionalSetup()
        {
            // Setup mocks
            _mockVnPayService = new Mock<IVnPayService>();
            _mockOrderService = new Mock<IOrderService>();
            _mockCartService = new Mock<ICartService>();

            // Setup authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestConstants.DefaultUserId.ToString()),
                new Claim(ClaimTypes.Name, TestConstants.DefaultUsername),
                new Claim(ClaimTypes.Role, "Customer")
            }, "mock"));

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;

            _controller = new CheckoutController(
                Context,
                _mockOrderService.Object,
                _mockCartService.Object,
                _mockVnPayService.Object
            );

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

        #region PlaceOrder with VNPay Tests

        [Test]
        public async Task PlaceOrder_WithVnPay_RedirectsToPaymentUrl()
        {
            // Arrange
            var cart = new List<CartItem>
            {
                TestDataBuilder.CreateCartItem(TestConstants.IPhone14ProductId, 1, TestConstants.IPhone14Price)
            };

            var order = new Order
            {
                OrderID = 1,
                UserID = TestConstants.DefaultUserId,
                TotalAmount = TestConstants.IPhone14Price,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "Test Address",
                PaymentMethod = "VnPay",
                PaymentStatus = "Pending"
            };

            var expectedPaymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=100000000&vnp_SecureHash=abc123";

            _mockCartService.Setup(s => s.GetCartItems(It.IsAny<HttpContext>()))
                .Returns(cart);
            _mockCartService.Setup(s => s.GetCartTotal(It.IsAny<HttpContext>()))
                .Returns(TestConstants.IPhone14Price);
            _mockOrderService.Setup(s => s.CreateOrderAsync(
                It.IsAny<int>(),
                It.IsAny<List<CartItem>>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(order);
            _mockVnPayService.Setup(s => s.CreatePaymentUrl(It.IsAny<HttpContext>(), It.IsAny<Order>()))
                .Returns(expectedPaymentUrl);

            // Act
            var result = await _controller.PlaceOrder("VnPay", "Test Address");

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult!.Url, Is.EqualTo(expectedPaymentUrl));

            // Verify service calls
            _mockOrderService.Verify(s => s.CreateOrderAsync(
                TestConstants.DefaultUserId,
                It.IsAny<List<CartItem>>(),
                "Test Address",
                "VnPay"), Times.Once);
            _mockVnPayService.Verify(s => s.CreatePaymentUrl(It.IsAny<HttpContext>(), order), Times.Once);
            _mockCartService.Verify(s => s.ClearCart(It.IsAny<HttpContext>()), Times.Once);
        }

        [Test]
        public async Task PlaceOrder_WithCOD_DoesNotCallVnPayService()
        {
            // Arrange
            var cart = new List<CartItem>
            {
                TestDataBuilder.CreateCartItem(TestConstants.IPhone14ProductId, 1, TestConstants.IPhone14Price)
            };

            var order = new Order
            {
                OrderID = 1,
                UserID = TestConstants.DefaultUserId,
                TotalAmount = TestConstants.IPhone14Price,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "Test Address",
                PaymentMethod = "COD",
                PaymentStatus = "Pending"
            };

            _mockCartService.Setup(s => s.GetCartItems(It.IsAny<HttpContext>()))
                .Returns(cart);
            _mockOrderService.Setup(s => s.CreateOrderAsync(
                It.IsAny<int>(),
                It.IsAny<List<CartItem>>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.PlaceOrder("COD", "Test Address");

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("OrderConfirmation"));

            // Verify VnPay service was NOT called
            _mockVnPayService.Verify(s => s.CreatePaymentUrl(It.IsAny<HttpContext>(), It.IsAny<Order>()), Times.Never);
        }

        #endregion

        #region VnPayReturn Tests

        [Test]
        public async Task VnPayReturn_SuccessfulPayment_UpdatesOrderAndRedirects()
        {
            // Arrange
            var orderId = 123;
            var response = new VnPayResponseModel
            {
                Success = true,
                OrderId = orderId.ToString(),
                TransactionId = "14123456",
                ResponseCode = "00",
                SecureHash = "validhash"
            };

            var queryParams = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", orderId.ToString() },
                { "vnp_TransactionNo", "14123456" }
            });

            _controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?vnp_ResponseCode=00");
            
            _mockVnPayService.Setup(s => s.ProcessCallback(It.IsAny<IQueryCollection>()))
                .Returns(response);
            _mockOrderService.Setup(s => s.UpdatePaymentStatusAsync(orderId, "Completed"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.VnPayReturn();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("OrderConfirmation"));
            Assert.That(redirectResult.RouteValues!["orderId"], Is.EqualTo(orderId));

            // Verify order status was updated
            _mockOrderService.Verify(s => s.UpdatePaymentStatusAsync(orderId, "Completed"), Times.Once);

            // Verify TempData contains success message
            Assert.That(_controller.TempData["PaymentSuccess"], Is.Not.Null);
        }

        [Test]
        public async Task VnPayReturn_FailedPayment_UpdatesOrderAsFailed()
        {
            // Arrange
            var orderId = 456;
            var response = new VnPayResponseModel
            {
                Success = false,
                OrderId = orderId.ToString(),
                TransactionId = "14123457",
                ResponseCode = "24", // User cancelled
                SecureHash = "validhash"
            };

            _mockVnPayService.Setup(s => s.ProcessCallback(It.IsAny<IQueryCollection>()))
                .Returns(response);
            _mockOrderService.Setup(s => s.UpdatePaymentStatusAsync(orderId, "Failed"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.VnPayReturn();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            
            // Verify order status was updated to Failed
            _mockOrderService.Verify(s => s.UpdatePaymentStatusAsync(orderId, "Failed"), Times.Once);

            // Verify TempData contains error message
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
            Assert.That(_controller.TempData["Error"]!.ToString(), Does.Contain("24"));
        }

        [Test]
        public async Task VnPayReturn_InvalidOrderId_RedirectsToHome()
        {
            // Arrange
            var response = new VnPayResponseModel
            {
                Success = false,
                OrderId = "invalid",
                TransactionId = "",
                ResponseCode = "",
                SecureHash = ""
            };

            _mockVnPayService.Setup(s => s.ProcessCallback(It.IsAny<IQueryCollection>()))
                .Returns(response);

            // Act
            var result = await _controller.VnPayReturn();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));

            // Verify order was NOT updated
            _mockOrderService.Verify(s => s.UpdatePaymentStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task VnPayReturn_AllowsAnonymousAccess()
        {
            // Arrange - Create unauthenticated user
            var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext.HttpContext.User = unauthenticatedUser;

            var response = new VnPayResponseModel
            {
                Success = true,
                OrderId = "123",
                TransactionId = "14123456",
                ResponseCode = "00",
                SecureHash = "validhash"
            };

            _mockVnPayService.Setup(s => s.ProcessCallback(It.IsAny<IQueryCollection>()))
                .Returns(response);
            _mockOrderService.Setup(s => s.UpdatePaymentStatusAsync(123, "Completed"))
                .ReturnsAsync(true);

            // Act - Should not throw authorization exception
            var result = await _controller.VnPayReturn();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task VnPayFlow_CompleteFlow_CreatesOrderAndProcessesPayment()
        {
            // Arrange
            var cart = new List<CartItem>
            {
                TestDataBuilder.CreateCartItem(TestConstants.IPhone14ProductId, 2, TestConstants.IPhone14Price)
            };

            var order = new Order
            {
                OrderID = 999,
                UserID = TestConstants.DefaultUserId,
                TotalAmount = TestConstants.IPhone14Price * 2,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "123 Test St",
                PaymentMethod = "VnPay",
                PaymentStatus = "Pending"
            };

            _mockCartService.Setup(s => s.GetCartItems(It.IsAny<HttpContext>()))
                .Returns(cart);
            _mockOrderService.Setup(s => s.CreateOrderAsync(
                It.IsAny<int>(),
                It.IsAny<List<CartItem>>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(order);
            _mockVnPayService.Setup(s => s.CreatePaymentUrl(It.IsAny<HttpContext>(), It.IsAny<Order>()))
                .Returns("https://vnpay.test/payment");

            // Act - Step 1: Place order
            var placeOrderResult = await _controller.PlaceOrder("VnPay", "123 Test St");

            // Assert - Step 1
            Assert.That(placeOrderResult, Is.InstanceOf<RedirectResult>());
            _mockOrderService.Verify(s => s.CreateOrderAsync(
                TestConstants.DefaultUserId,
                It.IsAny<List<CartItem>>(),
                "123 Test St",
                "VnPay"), Times.Once);

            // Arrange - Step 2: VNPay callback
            var callbackResponse = new VnPayResponseModel
            {
                Success = true,
                OrderId = order.OrderID.ToString(),
                TransactionId = "14123456",
                ResponseCode = "00",
                SecureHash = "validhash"
            };

            _mockVnPayService.Setup(s => s.ProcessCallback(It.IsAny<IQueryCollection>()))
                .Returns(callbackResponse);
            _mockOrderService.Setup(s => s.UpdatePaymentStatusAsync(order.OrderID, "Completed"))
                .ReturnsAsync(true);

            // Act - Step 2: Process callback
            var callbackResult = await _controller.VnPayReturn();

            // Assert - Step 2
            Assert.That(callbackResult, Is.InstanceOf<RedirectToActionResult>());
            _mockOrderService.Verify(s => s.UpdatePaymentStatusAsync(order.OrderID, "Completed"), Times.Once);
            Assert.That(_controller.TempData["PaymentSuccess"], Is.Not.Null);
        }

        #endregion
    }
}
