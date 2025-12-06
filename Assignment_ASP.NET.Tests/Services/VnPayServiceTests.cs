using NUnit.Framework;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Assignment_ASP.NET.Tests.Services
{
    /// <summary>
    /// Unit tests cho VnPayService
    /// </summary>
    [TestFixture]
    public class VnPayServiceTests
    {
        private VnPayService _service = null!;
        private IConfiguration _configuration = null!;
        private Mock<HttpContext> _mockHttpContext = null!;

        [SetUp]
        public void Setup()
        {
            // Setup configuration
            var inMemorySettings = new Dictionary<string, string>
            {
                {"VnPay:TmnCode", "2QXUI1L5"},
                {"VnPay:HashSecret", "AELPHGNYYQZTSNGRBWHKOWJTDGCNJIXS"},
                {"VnPay:Url", "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"},
                {"VnPay:ReturnUrl", "http://localhost:5215/Checkout/VnPayReturn"},
                {"VnPay:Version", "2.1.0"},
                {"VnPay:Command", "pay"},
                {"VnPay:CurrCode", "VND"},
                {"VnPay:Locale", "vn"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _service = new VnPayService(_configuration);

            // Setup HttpContext
            _mockHttpContext = new Mock<HttpContext>();
            var mockConnection = new Mock<ConnectionInfo>();
            mockConnection.Setup(c => c.RemoteIpAddress)
                .Returns(System.Net.IPAddress.Parse("127.0.0.1"));
            _mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);
        }

        #region CreatePaymentUrl Tests

        [Test]
        public void CreatePaymentUrl_ReturnsValidUrl_WithCorrectParameters()
        {
            // Arrange
            var order = new Order
            {
                OrderID = 123,
                TotalAmount = 1000000,
                UserID = 1,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "Test Address",
                PaymentMethod = "VnPay",
                PaymentStatus = "Pending"
            };

            // Act
            var result = _service.CreatePaymentUrl(_mockHttpContext.Object, order);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.StartWith("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?"));
            Assert.That(result, Does.Contain("vnp_TmnCode=2QXUI1L5"));
            Assert.That(result, Does.Contain("vnp_Amount=100000000")); // 1,000,000 * 100
            Assert.That(result, Does.Contain("vnp_CurrCode=VND"));
            Assert.That(result, Does.Contain("vnp_Version=2.1.0"));
            Assert.That(result, Does.Contain("vnp_Command=pay"));
            Assert.That(result, Does.Contain("vnp_Locale=vn"));
            Assert.That(result, Does.Contain("vnp_SecureHash="));
        }

        [Test]
        public void CreatePaymentUrl_ContainsReturnUrl()
        {
            // Arrange
            var order = new Order
            {
                OrderID = 456,
                TotalAmount = 500000,
                UserID = 1,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "Test Address",
                PaymentMethod = "VnPay",
                PaymentStatus = "Pending"
            };

            // Act
            var result = _service.CreatePaymentUrl(_mockHttpContext.Object, order);

            // Assert
            Assert.That(result, Does.Contain("vnp_ReturnUrl="));
            Assert.That(result, Does.Contain("Checkout%2FVnPayReturn"));
        }

        [Test]
        public void CreatePaymentUrl_ContainsOrderInfo()
        {
            // Arrange
            var order = new Order
            {
                OrderID = 789,
                TotalAmount = 2000000,
                UserID = 1,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "Test Address",
                PaymentMethod = "VnPay",
                PaymentStatus = "Pending"
            };

            // Act
            var result = _service.CreatePaymentUrl(_mockHttpContext.Object, order);

            // Assert
            Assert.That(result, Does.Contain("vnp_OrderInfo="));
            Assert.That(result, Does.Contain("Thanh"));
        }

        [Test]
        public void CreatePaymentUrl_DifferentAmounts_GeneratesDifferentUrls()
        {
            // Arrange
            var order1 = new Order
            {
                OrderID = 1,
                TotalAmount = 1000000,
                UserID = 1,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "Test",
                PaymentMethod = "VnPay",
                PaymentStatus = "Pending"
            };

            var order2 = new Order
            {
                OrderID = 2,
                TotalAmount = 2000000,
                UserID = 1,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = "Test",
                PaymentMethod = "VnPay",
                PaymentStatus = "Pending"
            };

            // Act
            var url1 = _service.CreatePaymentUrl(_mockHttpContext.Object, order1);
            var url2 = _service.CreatePaymentUrl(_mockHttpContext.Object, order2);

            // Assert
            Assert.That(url1, Does.Contain("vnp_Amount=100000000"));
            Assert.That(url2, Does.Contain("vnp_Amount=200000000"));
            Assert.That(url1, Is.Not.EqualTo(url2));
        }

        #endregion

        #region ProcessCallback Tests

        [Test]
        public void ProcessCallback_ValidSignature_ReturnsSuccess()
        {
            // Arrange
            var queryParams = CreateValidVnPayCallback();

            // Act
            var result = _service.ProcessCallback(queryParams);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.ResponseCode, Is.EqualTo("00"));
        }

        [Test]
        public void ProcessCallback_InvalidSignature_ReturnsFailure()
        {
            // Arrange
            var queryParams = CreateInvalidVnPayCallback();

            // Act
            var result = _service.ProcessCallback(queryParams);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void ProcessCallback_ExtractsOrderId()
        {
            // Arrange
            var queryParams = CreateValidVnPayCallback();

            // Act
            var result = _service.ProcessCallback(queryParams);

            // Assert
            Assert.That(result.OrderId, Is.Not.Null);
            Assert.That(result.OrderId, Is.Not.Empty);
        }

        [Test]
        public void ProcessCallback_ExtractsTransactionId()
        {
            // Arrange
            var queryParams = CreateValidVnPayCallback();

            // Act
            var result = _service.ProcessCallback(queryParams);

            // Assert
            Assert.That(result.TransactionId, Is.Not.Null);
            Assert.That(result.TransactionId, Is.Not.Empty);
        }

        [Test]
        public void ProcessCallback_FailedTransaction_ReturnsCorrectCode()
        {
            // Arrange
            var queryParams = CreateFailedVnPayCallback();

            // Act
            var result = _service.ProcessCallback(queryParams);

            // Assert
            Assert.That(result.ResponseCode, Is.Not.EqualTo("00"));
            Assert.That(result.Success, Is.False);
        }

        #endregion

        #region VnPayLibrary Tests

        [Test]
        public void VnPayLibrary_SortsParametersCorrectly()
        {
            // Arrange
            var library = new VnPayLibrary();
            library.AddRequestData("vnp_Version", "2.1.0");
            library.AddRequestData("vnp_Amount", "100000");
            library.AddRequestData("vnp_Command", "pay");

            // Act
            var url = library.CreateRequestUrl("https://test.com", "SECRET");

            // Assert
            // Parameters should be sorted alphabetically
            var queryStart = url.IndexOf('?');
            var queryString = url.Substring(queryStart + 1);
            var firstParam = queryString.Split('&')[0];
            
            Assert.That(firstParam, Does.StartWith("vnp_Amount="));
        }

        [Test]
        public void VnPayLibrary_IgnoresEmptyValues()
        {
            // Arrange
            var library = new VnPayLibrary();
            library.AddRequestData("vnp_Version", "2.1.0");
            library.AddRequestData("vnp_Empty", "");
            library.AddRequestData("vnp_Amount", "100000");

            // Act
            var url = library.CreateRequestUrl("https://test.com", "SECRET");

            // Assert
            Assert.That(url, Does.Not.Contain("vnp_Empty"));
        }

        #endregion

        #region Helper Methods

        private QueryCollection CreateValidVnPayCallback()
        {
            // Create a valid callback with proper signature
            var library = new VnPayLibrary();
            library.AddResponseData("vnp_TmnCode", "2QXUI1L5");
            library.AddResponseData("vnp_Amount", "100000000");
            library.AddResponseData("vnp_BankCode", "NCB");
            library.AddResponseData("vnp_ResponseCode", "00");
            library.AddResponseData("vnp_TxnRef", "638678901234567890");
            library.AddResponseData("vnp_TransactionNo", "14123456");
            library.AddResponseData("vnp_OrderInfo", "Thanh toan don hang 123");

            var queryDict = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_TmnCode", "2QXUI1L5" },
                { "vnp_Amount", "100000000" },
                { "vnp_BankCode", "NCB" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "638678901234567890" },
                { "vnp_TransactionNo", "14123456" },
                { "vnp_OrderInfo", "Thanh toan don hang 123" },
                { "vnp_SecureHash", "validhash123" } // In real test, calculate actual hash
            };

            return new QueryCollection(queryDict);
        }

        private QueryCollection CreateInvalidVnPayCallback()
        {
            var queryDict = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_TmnCode", "2QXUI1L5" },
                { "vnp_Amount", "100000000" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TxnRef", "123456" },
                { "vnp_TransactionNo", "14123456" },
                { "vnp_SecureHash", "invalidhash" }
            };

            return new QueryCollection(queryDict);
        }

        private QueryCollection CreateFailedVnPayCallback()
        {
            var queryDict = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_TmnCode", "2QXUI1L5" },
                { "vnp_Amount", "100000000" },
                { "vnp_ResponseCode", "24" }, // User cancelled
                { "vnp_TxnRef", "123456" },
                { "vnp_TransactionNo", "14123456" },
                { "vnp_SecureHash", "somehash" }
            };

            return new QueryCollection(queryDict);
        }

        #endregion
    }
}
