using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Assignment_ASP.NET.Tests.Controllers
{
    [TestFixture]
    public class UsersControllerTests
    {
        private ApplicationDbContext _context;
        private UsersController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Users_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed Roles
            _context.Roles.Add(new Role { RoleID = 1, RoleName = "Admin" });
            _context.Roles.Add(new Role { RoleID = 2, RoleName = "Employee" });

            // Seed User
            var sha256 = SHA256.Create();
            var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("123456"))).Replace("-", "").ToLower();
            _context.Users.Add(new User { UserID = 1, Username = "admin", PasswordHash = passwordHash, RoleID = 1, Email = "admin@test.com", FullName = "Admin", Address = "Addr", Phone = "123" });

            _context.SaveChanges();

            _controller = new UsersController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithUsers()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<User>;
            Assert.That(model.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Create_Post_ValidUser_RedirectsToIndex()
        {
            // Arrange
            var newUser = new User { Username = "newuser", FullName = "New User", Email = "new@test.com", Address = "Addr", Phone = "123", RoleID = 2 };

            // Act
            var result = await _controller.Create(newUser, "password");

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

            // Verify User added and password hashed
            var addedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.That(addedUser, Is.Not.Null);
            Assert.That(addedUser.PasswordHash, Is.Not.Null);
            Assert.That(addedUser.PasswordHash, Is.Not.EqualTo("password"));
        }

        [Test]
        public async Task Edit_Post_UpdatesUser_And_Password()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            user.FullName = "Updated Name";

            // Act
            var result = await _controller.Edit(user.UserID, user, "newpassword");

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            
            var updatedUser = await _context.Users.FindAsync(user.UserID);
            Assert.That(updatedUser.FullName, Is.EqualTo("Updated Name"));
            
            // Verify password changed
            // We know the hash of "123456" from setup.
            // We expect a different hash now.
             using (var sha256 = SHA256.Create())
            {
                var expectedHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("newpassword"))).Replace("-", "").ToLower();
                Assert.That(updatedUser.PasswordHash, Is.EqualTo(expectedHash));
            }
        }

        [Test]
        public async Task DeleteConfirmed_Fails_IfUserHasOrders()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            _context.Orders.Add(new Order { OrderID = 1, UserID = user.UserID, OrderDate = DateTime.Now, Status = "Pending", TotalAmount = 100, ShippingAddress = "Address" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(user.UserID);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>()); // Returns View("Delete", user) with error
            var viewResult = result as ViewResult;
            Assert.That(viewResult.ViewName, Is.EqualTo("Delete"));
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_Succeeds_IfNoOrders()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_context.Users.Count(), Is.EqualTo(0));
        }
    }
}
