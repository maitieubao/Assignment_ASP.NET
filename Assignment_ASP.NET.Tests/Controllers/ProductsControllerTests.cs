using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Assignment_ASP.NET.Tests.Controllers
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private ApplicationDbContext _context;
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private ProductsController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Products_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed data
            _context.Categories.Add(new Category { CategoryID = 1, CategoryName = "Phone" });
            _context.Products.Add(new Product { ProductID = 1, ProductName = "iPhone 14", CategoryID = 1, Price = 1000, ImageUrl = "/images/test.jpg" });
            _context.SaveChanges();

            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(m => m.WebRootPath).Returns("wwwroot");

            _controller = new ProductsController(_context, _mockEnvironment.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithProducts()
        {
            // Act
            var result = await _controller.Index(null, null, null);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult.Model as List<Product>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Details_ReturnsViewResult_WithProduct()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult.Model as Product;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.ProductID, Is.EqualTo(1));
        }

        [Test]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var product = new Product { ProductName = "New Product", Price = 500, CategoryID = 1 };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.Length).Returns(1024);
            
            // Mock file copy
            // Since UploadFile uses file.CopyToAsync, we need to mock it if we want to verify it, 
            // but for this test we just want to ensure it runs. 
            // However, UploadFile creates a FileStream which might fail if directory doesn't exist or path is invalid.
            // We mocked WebRootPath to "wwwroot". We should ensure directory creation doesn't fail or we mock FileStream? 
            // We can't easily mock FileStream constructor.
            // We can rely on the fact that InMemory DB doesn't actually write files to disk for the DB part, 
            // but the controller code DOES write to disk.
            // We should probably skip file upload test or use a real temp directory for WebRootPath.
            
            // Let's use a temp directory for WebRootPath
            var tempPath = Path.Combine(Path.GetTempPath(), "TestWebRoot_" + System.Guid.NewGuid());
            Directory.CreateDirectory(tempPath);
            _mockEnvironment.Setup(m => m.WebRootPath).Returns(tempPath);

            // Act
            var result = await _controller.Create(product, mockFile.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            
            Assert.That(_context.Products.Count(), Is.EqualTo(2));

            // Cleanup
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }

        [Test]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var product = await _context.Products.FirstAsync();
            product.ProductName = "Updated Product";

            // Act
            var result = await _controller.Edit(product.ProductID, product, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

            var updatedProduct = await _context.Products.FindAsync(product.ProductID);
            Assert.That(updatedProduct.ProductName, Is.EqualTo("Updated Product"));
        }

        [Test]
        public async Task Delete_Post_RedirectsToIndex()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_context.Products.Count(), Is.EqualTo(0));
        }
    }
}
