using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho ProductsController
    /// </summary>
    [TestFixture]
    public class ProductsControllerTests : ControllerTestBase
    {
        private Mock<IWebHostEnvironment> _mockEnvironment = null!;
        private ProductsController _controller = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Products";

        protected override void SeedCommonData()
        {
            // Seed categories và products cho product management tests
            SeedCategories();
            
            // Seed một product cụ thể cho tests
            Context.Products.Add(TestDataBuilder.CreateProduct(
                TestConstants.IPhone14ProductId,
                TestConstants.IPhone14ProductName,
                TestConstants.PhoneCategoryId,
                TestConstants.IPhone14Price
            ));
            Context.SaveChanges();
        }

        protected override void AdditionalSetup()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(m => m.WebRootPath).Returns("wwwroot");

            _controller = new ProductsController(Context, _mockEnvironment.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }

        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithProducts()
        {
            // Act
            var result = await _controller.Index(null, null, null);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as List<Product>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(1), "Should return 1 product");
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
            var model = viewResult!.Model as Product;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.ProductID, Is.EqualTo(TestConstants.IPhone14ProductId));
            Assert.That(model.ProductName, Is.EqualTo(TestConstants.IPhone14ProductName));
        }

        #endregion

        #region Create Tests

        [Test]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var product = new Product 
            { 
                ProductName = "New Product", 
                Price = 500, 
                CategoryID = TestConstants.PhoneCategoryId 
            };
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.Length).Returns(1024);
            
            // Use a temp directory for WebRootPath to avoid file system issues
            var tempPath = Path.Combine(Path.GetTempPath(), "TestWebRoot_" + System.Guid.NewGuid());
            Directory.CreateDirectory(tempPath);
            _mockEnvironment.Setup(m => m.WebRootPath).Returns(tempPath);

            // Act
            var result = await _controller.Create(product, mockFile.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo(TestConstants.IndexAction));
            
            Assert.That(Context.Products.Count(), Is.EqualTo(2), "Should have 2 products after adding new one");

            // Cleanup
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }

        #endregion

        #region Edit Tests

        [Test]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var product = Context.Products.First();
            var originalName = product.ProductName;
            product.ProductName = "Updated Product";

            // Act
            var result = await _controller.Edit(product.ProductID, product, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo(TestConstants.IndexAction));

            var updatedProduct = await Context.Products.FindAsync(product.ProductID);
            Assert.That(updatedProduct, Is.Not.Null);
            Assert.That(updatedProduct!.ProductName, Is.EqualTo("Updated Product"));
            Assert.That(updatedProduct.ProductName, Is.Not.EqualTo(originalName), "Product name should be updated");
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task Delete_Post_RedirectsToIndex()
        {
            // Arrange
            var initialCount = Context.Products.Count();

            // Act
            var result = await _controller.DeleteConfirmed(TestConstants.IPhone14ProductId);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(Context.Products.Count(), Is.EqualTo(initialCount - 1), "Should have one less product");
            
            var deletedProduct = await Context.Products.FindAsync(TestConstants.IPhone14ProductId);
            Assert.That(deletedProduct, Is.Null, "Deleted product should not exist");
        }

        #endregion
    }
}
