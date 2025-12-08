using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assignment_ASP.NET.Tests
{
    public class CategoriesControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            // Tạo In-Memory Database cho testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new CategoriesController(_context);

            // Seed dữ liệu test
            SeedTestData();
        }

        private void SeedTestData()
        {
            var categories = new List<Category>
            {
                new Category { CategoryID = 1, CategoryName = "Smartphone" },
                new Category { CategoryID = 2, CategoryName = "Tablet" },
                new Category { CategoryID = 3, CategoryName = "Laptop" }
            };

            _context.Categories.AddRange(categories);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Details Tests

        [Fact]
        public async Task Details_WithValidId_ReturnsViewResult()
        {
            // Arrange
            int categoryId = 1;

            // Act
            var result = await _controller.Details(categoryId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Category>(viewResult.Model);
            Assert.Equal(categoryId, model.CategoryID);
            Assert.Equal("Smartphone", model.CategoryName);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var newCategory = new Category
            {
                CategoryName = "Smartwatch"
            };

            // Act
            var result = await _controller.Create(newCategory);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify category was added
            var categories = await _context.Categories.ToListAsync();
            Assert.Equal(4, categories.Count);
            Assert.Contains(categories, c => c.CategoryName == "Smartwatch");
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            // Arrange
            int categoryId = 1;

            // Act
            var result = await _controller.DeleteConfirmed(categoryId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify category was deleted
            var categories = await _context.Categories.ToListAsync();
            Assert.Equal(2, categories.Count);
            Assert.DoesNotContain(categories, c => c.CategoryID == categoryId);
        }

        #endregion
    }
}
