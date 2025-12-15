using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Data;

namespace Assignment_ASP.NET.Tests;

[TestFixture]
public class ProductsControllerTests
{
    private Mock<IProductService> _mockService = null!;
    private ApplicationDbContext _context = null!;
    private ProductsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IProductService>();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_Products_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        
        _controller = new ProductsController(_mockService.Object, _context);
    }

    [Test]
    public async Task Index_WhenCalled_ReturnsViewWithProducts()
    {
        _mockService.Setup(s => s.GetProductsAsync(null!, null, 1, 10))
            .ReturnsAsync((new List<Product> { new Product { ProductID = 1, ProductName = "iPhone" } }, 1, 1));
        
        var result = await _controller.Index(null, null, null);
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Details_ExistingId_ReturnsViewWithProduct()
    {
        _mockService.Setup(s => s.GetProductByIdAsync(1))
            .ReturnsAsync(new Product { ProductID = 1, ProductName = "iPhone" });
        _mockService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(new List<Category>());
        
        var result = await _controller.Details(1);
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.Model, Is.InstanceOf<Product>());
    }

    [Test]
    public async Task Details_UnknownId_ReturnsNotFoundResult()
    {
        _mockService.Setup(s => s.GetProductByIdAsync(999))
            .ReturnsAsync((Product?)null);
        
        var result = await _controller.Details(999);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
        _context?.Dispose();
    }
}
