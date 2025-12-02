using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho HomeController
    /// </summary>
    [TestFixture]
    public class HomeControllerTests : ControllerTestBase
    {
        private HomeController _controller = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Home";

        protected override void SeedCommonData()
        {
            // Seed categories và products cho home page tests
            SeedCategories();
            SeedProducts();
        }

        protected override void AdditionalSetup()
        {
            _controller = new HomeController(Context);
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }

        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithAllProducts()
        {
            // Act
            var result = await _controller.Index(null, null, null, null, null, 1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.InstanceOf<HomeIndexViewModel>());
            var model = viewResult.Model as HomeIndexViewModel;
            Assert.That(model, Is.Not.Null);
            
            Assert.That(model!.Products.Count(), Is.EqualTo(3), "Should return all 3 products");
            Assert.That(model.Categories.Count(), Is.EqualTo(2), "Should return all 2 categories");
        }

        [Test]
        public async Task Index_ReturnsFilteredProducts_BySearchString()
        {
            // Act
            var result = await _controller.Index(TestConstants.IPhone14ProductName, null, null, null, null, 1);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as HomeIndexViewModel;
            Assert.That(model, Is.Not.Null);

            Assert.That(model!.Products.Count(), Is.EqualTo(1), "Should return only iPhone 14");
            Assert.That(model.Products.First().ProductName, Is.EqualTo(TestConstants.IPhone14ProductName));
        }

        [Test]
        public async Task Index_ReturnsFilteredProducts_ByCategoryId()
        {
            // Act
            var result = await _controller.Index(null, TestConstants.LaptopCategoryId, null, null, null, 1);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as HomeIndexViewModel;
            Assert.That(model, Is.Not.Null);

            Assert.That(model!.Products.Count(), Is.EqualTo(1), "Should return only MacBook Pro");
            Assert.That(model.Products.First().ProductName, Is.EqualTo(TestConstants.MacBookProProductName));
        }

        #endregion

        #region Details Tests

        [Test]
        public async Task Details_ReturnsViewResult_WithProduct()
        {
            // Act
            var result = await _controller.Details(TestConstants.IPhone14ProductId);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.InstanceOf<Product>());
            var model = viewResult.Model as Product;
            Assert.That(model, Is.Not.Null);

            Assert.That(model!.ProductID, Is.EqualTo(TestConstants.IPhone14ProductId));
            Assert.That(model.ProductName, Is.EqualTo(TestConstants.IPhone14ProductName));
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>(), "Should return NotFound when ID is null");
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenProductNotFound()
        {
            // Arrange
            const int nonExistentId = 999;

            // Act
            var result = await _controller.Details(nonExistentId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>(), "Should return NotFound when product doesn't exist");
        }

        #endregion
    }
}
