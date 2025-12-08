using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assignment_ASP.NET.Tests
{
    public class CouponsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CouponsController _controller;

        public CouponsControllerTests()
        {
            // Tạo In-Memory Database cho testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new CouponsController(_context);

            // Seed dữ liệu test
            SeedTestData();
        }

        private void SeedTestData()
        {
            var coupons = new List<Coupon>
            {
                new Coupon 
                { 
                    CouponID = 1, 
                    Code = "SALE10", 
                    DiscountPercentage = 10, 
                    ExpiryDate = DateTime.Now.AddDays(30),
                    IsActive = true 
                },
                new Coupon 
                { 
                    CouponID = 2, 
                    Code = "SALE20", 
                    DiscountPercentage = 20, 
                    ExpiryDate = DateTime.Now.AddDays(60),
                    IsActive = true 
                },
                new Coupon 
                { 
                    CouponID = 3, 
                    Code = "EXPIRED", 
                    DiscountPercentage = 15, 
                    ExpiryDate = DateTime.Now.AddDays(-10),
                    IsActive = false 
                }
            };

            _context.Coupons.AddRange(coupons);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Create Tests

        [Fact]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var newCoupon = new Coupon
            {
                Code = "NEWYEAR2025",
                DiscountPercentage = 25,
                ExpiryDate = DateTime.Now.AddDays(90),
                IsActive = true
            };

            // Act
            var result = await _controller.Create(newCoupon);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify coupon was added
            var coupons = await _context.Coupons.ToListAsync();
            Assert.Equal(4, coupons.Count);
            Assert.Contains(coupons, c => c.Code == "NEWYEAR2025");
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var coupon = await _context.Coupons.FindAsync(1);
            coupon.DiscountPercentage = 15;
            coupon.Code = "SALE15";

            // Act
            var result = await _controller.Edit(1, coupon);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify coupon was updated
            var updatedCoupon = await _context.Coupons.FindAsync(1);
            Assert.Equal(15, updatedCoupon.DiscountPercentage);
            Assert.Equal("SALE15", updatedCoupon.Code);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            // Arrange
            int couponId = 1;

            // Act
            var result = await _controller.DeleteConfirmed(couponId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify coupon was deleted
            var coupons = await _context.Coupons.ToListAsync();
            Assert.Equal(2, coupons.Count);
            Assert.DoesNotContain(coupons, c => c.CouponID == couponId);
        }

        #endregion
    }
}
