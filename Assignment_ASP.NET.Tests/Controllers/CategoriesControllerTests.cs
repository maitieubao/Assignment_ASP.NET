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
    public class CategoriesControllerTests
    {
        private ApplicationDbContext _context;
        private CategoriesController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Categories_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed data
            _context.Categories.Add(new Category { CategoryID = 1, CategoryName = "Phone" });
            _context.Categories.Add(new Category { CategoryID = 2, CategoryName = "Laptop" });
            _context.SaveChanges();

            _controller = new CategoriesController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCategories()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<Category>;
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Details_ReturnsViewResult_WithCategory()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Category;
            Assert.That(model.CategoryID, Is.EqualTo(1));
        }

        [Test]
        public async Task Create_Post_ValidCategory_RedirectsToIndex()
        {
            // Arrange
            var category = new Category { CategoryName = "Tablet" };

            // Act
            var result = await _controller.Create(category);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_context.Categories.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task Edit_Post_ValidCategory_RedirectsToIndex()
        {
            // Arrange
            var category = await _context.Categories.FirstAsync();
            category.CategoryName = "Smart Phone";

            // Act
            var result = await _controller.Edit(category.CategoryID, category);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var updatedCategory = await _context.Categories.FindAsync(category.CategoryID);
            Assert.That(updatedCategory.CategoryName, Is.EqualTo("Smart Phone"));
        }

        [Test]
        public async Task DeleteConfirmed_RemovesCategory()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_context.Categories.Count(), Is.EqualTo(1));
        }
    }
}
