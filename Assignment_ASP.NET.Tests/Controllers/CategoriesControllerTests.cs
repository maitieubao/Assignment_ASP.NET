using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho CategoriesController
    /// </summary>
    [TestFixture]
    public class CategoriesControllerTests : ControllerTestBase
    {
        private CategoriesController _controller = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Categories";

        protected override void SeedCommonData()
        {
            // Seed categories cho tất cả tests
            SeedCategories();
        }

        protected override void AdditionalSetup()
        {
            _controller = new CategoriesController(Context);
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }


        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithCategories()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<Category>;
            Assert.That(model.Count, Is.EqualTo(2), "Should return 2 categories");
        }

        #endregion

        #region Details Tests

        [Test]
        public async Task Details_ReturnsViewResult_WithCategory()
        {
            // Act
            var result = await _controller.Details(TestConstants.PhoneCategoryId);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Category;
            Assert.That(model.CategoryID, Is.EqualTo(TestConstants.PhoneCategoryId));
            Assert.That(model.CategoryName, Is.EqualTo(TestConstants.PhoneCategoryName));
        }

        #endregion

        #region Create Tests

        [Test]
        public async Task Create_Post_ValidCategory_RedirectsToIndex()
        {
            // Arrange
            var category = new Category { CategoryName = "Tablet" };

            // Act
            var result = await _controller.Create(category);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo(TestConstants.IndexAction));
            Assert.That(Context.Categories.Count(), Is.EqualTo(3), "Should have 3 categories after adding new one");
        }

        #endregion

        #region Edit Tests

        [Test]
        public async Task Edit_Post_ValidCategory_RedirectsToIndex()
        {
            // Arrange
            var category = Context.Categories.First();
            var originalName = category.CategoryName;
            category.CategoryName = "Smart Phone";

            // Act
            var result = await _controller.Edit(category.CategoryID, category);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var updatedCategory = await Context.Categories.FindAsync(category.CategoryID);
            Assert.That(updatedCategory.CategoryName, Is.EqualTo("Smart Phone"));
            Assert.That(updatedCategory.CategoryName, Is.Not.EqualTo(originalName), "Category name should be updated");
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task DeleteConfirmed_RemovesCategory()
        {
            // Arrange
            var initialCount = Context.Categories.Count();

            // Act
            var result = await _controller.DeleteConfirmed(TestConstants.PhoneCategoryId);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(Context.Categories.Count(), Is.EqualTo(initialCount - 1), "Should have one less category");
            
            var deletedCategory = await Context.Categories.FindAsync(TestConstants.PhoneCategoryId);
            Assert.That(deletedCategory, Is.Null, "Deleted category should not exist");
        }

        #endregion
    }
}
