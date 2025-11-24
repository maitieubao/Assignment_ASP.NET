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
    public class HomeControllerTests
    {
        private ApplicationDbContext _context;
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + System.Guid.NewGuid()) // Unique DB for each test
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed data
            _context.Categories.Add(new Category { CategoryID = 1, CategoryName = "Phone" });
            _context.Categories.Add(new Category { CategoryID = 2, CategoryName = "Laptop" });

            _context.Products.Add(new Product { ProductID = 1, ProductName = "iPhone 14", CategoryID = 1, Price = 1000 });
            _context.Products.Add(new Product { ProductID = 2, ProductName = "Samsung S23", CategoryID = 1, Price = 900 });
            _context.Products.Add(new Product { ProductID = 3, ProductName = "MacBook Pro", CategoryID = 2, Price = 2000 });

            _context.SaveChanges();

            _controller = new HomeController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithAllProducts()
        {
            // Act
            var result = await _controller.Index(null, null);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<HomeIndexViewModel>());
            var model = viewResult.Model as HomeIndexViewModel;
            
            Assert.That(model.Products.Count(), Is.EqualTo(3));
            Assert.That(model.Categories.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Index_ReturnsFilteredProducts_BySearchString()
        {
            // Act
            var result = await _controller.Index("iPhone", null);

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult.Model as HomeIndexViewModel;

            Assert.That(model.Products.Count(), Is.EqualTo(1));
            Assert.That(model.Products.First().ProductName, Is.EqualTo("iPhone 14"));
        }

        [Test]
        public async Task Index_ReturnsFilteredProducts_ByCategoryId()
        {
            // Act
            var result = await _controller.Index(null, 2);

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult.Model as HomeIndexViewModel;

            Assert.That(model.Products.Count(), Is.EqualTo(1));
            Assert.That(model.Products.First().ProductName, Is.EqualTo("MacBook Pro"));
        }

        [Test]
        public async Task Details_ReturnsViewResult_WithProduct()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<Product>());
            var model = viewResult.Model as Product;

            Assert.That(model.ProductID, Is.EqualTo(1));
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenProductNotFound()
        {
            // Act
            var result = await _controller.Details(99);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
