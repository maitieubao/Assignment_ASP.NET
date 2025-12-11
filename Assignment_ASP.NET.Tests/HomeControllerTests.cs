using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Assignment_ASP.NET.Tests;

[TestFixture]
public class HomeControllerTests
{
    private Mock<IProductService> _mockService = null!;
    private HomeController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IProductService>();
        _controller = new HomeController(_mockService.Object);
    }

    [Test]
    public async Task Index_ReturnsView()
    {
        _mockService.Setup(s => s.GetHomeProductsAsync(null!, null, null!, null, null, 1, 20))
            .ReturnsAsync((new List<Product> { new Product { ProductID = 1, ProductName = "iPhone" } }, 1, 1, 1));
        _mockService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(new List<Category> { new Category { CategoryID = 1, CategoryName = "Phone" } });
        
        var result = await _controller.Index(null, null, null, null, null, 1);
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Details_ReturnsProduct()
    {
        _mockService.Setup(s => s.GetProductWithReviewsAsync(1))
            .ReturnsAsync(new Product { ProductID = 1, ProductName = "iPhone" });
        
        var result = await _controller.Details(1);
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.Model, Is.InstanceOf<Product>());
    }

    [Test]
    public async Task Details_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetProductWithReviewsAsync(999))
            .ReturnsAsync((Product?)null);
        
        var result = await _controller.Details(999);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }
}
