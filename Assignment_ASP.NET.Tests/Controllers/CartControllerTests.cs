using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho CartController
    /// </summary>
    [TestFixture]
    public class CartControllerTests : ControllerTestBase
    {
        private CartController _controller = null!;
        private Mock<ISession> _mockSession = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Cart";

        protected override void SeedCommonData()
        {
            // Seed products cho cart tests
            SeedProducts();
        }

        protected override void AdditionalSetup()
        {
            _mockSession = new Mock<ISession>();
            
            var httpContext = new DefaultHttpContext();
            httpContext.Session = _mockSession.Object;

            _controller = new CartController(Context);
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

        #region Add Tests

        [Test]
        public async Task Add_AddsNewItemToCart_WhenCartIsEmpty()
        {
            // Arrange
            SessionHelper.SetupEmptyCart(_mockSession);

            // Act
            var result = await _controller.Add(TestConstants.IPhone14ProductId, 1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            SessionHelper.VerifyCartSet(_mockSession, Times.Once());
        }

        [Test]
        public async Task Add_IncrementsQuantity_WhenItemExists()
        {
            // Arrange
            var existingCart = new List<CartItem>
            {
                TestDataBuilder.CreateCartItem(TestConstants.IPhone14ProductId, 1, TestConstants.IPhone14Price)
            };
            SessionHelper.SetupCartWithItems(_mockSession, existingCart);

            // Act
            await _controller.Add(TestConstants.IPhone14ProductId, 1);

            // Assert
            SessionHelper.VerifyCartSetWithQuantity(_mockSession, TestConstants.IPhone14ProductId, 2);
        }

        #endregion

        #region Remove Tests

        [Test]
        public void Remove_RemovesItemFromCart()
        {
            // Arrange
            var existingCart = new List<CartItem>
            {
                TestDataBuilder.CreateCartItem(TestConstants.IPhone14ProductId, 1, TestConstants.IPhone14Price)
            };
            SessionHelper.SetupCartWithItems(_mockSession, existingCart);

            // Act
            var result = _controller.Remove(TestConstants.IPhone14ProductId);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            SessionHelper.VerifyCartSetWithItemCount(_mockSession, 0);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_UpdatesQuantity()
        {
            // Arrange
            var existingCart = new List<CartItem>
            {
                TestDataBuilder.CreateCartItem(TestConstants.IPhone14ProductId, 1, TestConstants.IPhone14Price)
            };
            SessionHelper.SetupCartWithItems(_mockSession, existingCart);
            const int newQuantity = 5;

            // Act
            var result = _controller.Update(TestConstants.IPhone14ProductId, newQuantity);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            SessionHelper.VerifyCartSetWithQuantity(_mockSession, TestConstants.IPhone14ProductId, newQuantity);
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_RemovesCartFromSession()
        {
            // Act
            var result = _controller.Clear();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            SessionHelper.VerifyCartRemoved(_mockSession, Times.Once());
        }

        #endregion
    }
}
