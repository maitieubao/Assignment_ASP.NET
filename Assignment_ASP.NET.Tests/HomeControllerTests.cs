using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Assignment_ASP.NET.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _controller = new HomeController(_mockProductService.Object);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithHomeIndexViewModel()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductID = 1, ProductName = "iPhone 15", Price = 20000000 },
                new Product { ProductID = 2, ProductName = "Samsung Galaxy S24", Price = 18000000 }
            };

            var categories = new List<Category>
            {
                new Category { CategoryID = 1, CategoryName = "Smartphone" },
                new Category { CategoryID = 2, CategoryName = "Tablet" }
            };

            _mockProductService.Setup(s => s.GetHomeProductsAsync(
                It.IsAny<string>(), 
                It.IsAny<int?>(), 
                It.IsAny<string>(), 
                It.IsAny<decimal?>(), 
                It.IsAny<decimal?>(), 
                It.IsAny<int>(), 
                It.IsAny<int>()))
                .ReturnsAsync((products, 1, 1, 2));

            _mockProductService.Setup(s => s.GetCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.Index(null, null, null, null, null, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<HomeIndexViewModel>(viewResult.Model);
            Assert.Equal(2, model.Products.Count());
            Assert.Equal(2, model.Categories.Count());
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_WithValidId_ReturnsViewResult()
        {
            // Arrange
            int productId = 1;
            var product = new Product 
            { 
                ProductID = productId, 
                ProductName = "iPhone 15", 
                Price = 20000000 
            };

            _mockProductService.Setup(s => s.GetProductWithReviewsAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.Details(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(productId, model.ProductID);
        }

        #endregion

        #region Promotions Tests

        [Fact]
        public async Task Promotions_ReturnsViewResult_WithActiveCoupons()
        {
            // Arrange
            var coupons = new List<Coupon>
            {
                new Coupon { CouponID = 1, Code = "SALE10", DiscountPercentage = 10, IsActive = true },
                new Coupon { CouponID = 2, Code = "SALE20", DiscountPercentage = 20, IsActive = true }
            };

            _mockProductService.Setup(s => s.GetActiveCouponsAsync())
                .ReturnsAsync(coupons);

            // Act
            var result = await _controller.Promotions();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Coupon>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        #endregion
    }
}
