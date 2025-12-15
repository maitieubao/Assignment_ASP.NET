using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment_ASP.NET.Tests;

[TestFixture]
public class CategoriesControllerTests
{
    private ApplicationDbContext _context = null!;
    private CategoriesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_Categories_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        
        _context.Categories.Add(new Category { CategoryID = 1, CategoryName = "Phone" });
        _context.SaveChanges();
        
        _controller = new CategoriesController(_context);
    }

    [Test]
    public async Task Index_WhenCalled_ReturnsViewWithCategories()
    {
        var result = await _controller.Index();
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.Model, Is.Not.Null);
    }

    [Test]
    public async Task Create_ValidCategory_RedirectsToIndexAndAddsCategory()
    {
        var category = new Category { CategoryName = "Laptop" };
        var result = await _controller.Create(category);
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        Assert.That(_context.Categories.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteConfirmed_ExistingId_RedirectsToIndexAndRemovesCategory()
    {
        var result = await _controller.DeleteConfirmed(1);
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        Assert.That(_context.Categories.Count(), Is.EqualTo(0));
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
        _context?.Dispose();
    }
}
