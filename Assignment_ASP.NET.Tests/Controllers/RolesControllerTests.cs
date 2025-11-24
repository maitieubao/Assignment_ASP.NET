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
    public class RolesControllerTests
    {
        private ApplicationDbContext _context;
        private RolesController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Roles_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed data
            _context.Roles.Add(new Role { RoleID = 1, RoleName = "Admin" });
            _context.Roles.Add(new Role { RoleID = 2, RoleName = "Employee" });
            _context.SaveChanges();

            _controller = new RolesController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithRoles()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<Role>;
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Create_Post_ValidRole_RedirectsToIndex()
        {
            // Arrange
            var role = new Role { RoleName = "Customer" };

            // Act
            var result = await _controller.Create(role);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_context.Roles.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task Create_Post_DuplicateRoleName_ReturnsView()
        {
            // Arrange
            var role = new Role { RoleName = "Admin" };

            // Act
            var result = await _controller.Create(role);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_Fails_IfRoleHasUsers()
        {
            // Arrange
            var role = await _context.Roles.FirstAsync();
            _context.Users.Add(new User { UserID = 1, Username = "user", PasswordHash = "hash", RoleID = role.RoleID, FullName = "User", Email = "email", Address = "addr", Phone = "123" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(role.RoleID);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.ViewName, Is.EqualTo("Delete"));
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_Succeeds_IfNoUsers()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_context.Roles.Count(), Is.EqualTo(1));
        }
    }
}
