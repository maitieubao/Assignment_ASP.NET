using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Assignment_ASP.NET.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private ApplicationDbContext _context;
        private AccountController _controller;
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IServiceProvider> _serviceProviderMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Account_" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed Roles
            _context.Roles.Add(new Role { RoleID = 1, RoleName = "Admin" });
            _context.Roles.Add(new Role { RoleID = 3, RoleName = "Customer" });
            
            // Seed User
            var sha256 = SHA256.Create();
            var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("123456"))).Replace("-", "").ToLower();
            _context.Users.Add(new User { UserID = 1, Username = "admin", PasswordHash = passwordHash, RoleID = 1, Email = "admin@test.com", FullName = "Admin", Address = "Addr", Phone = "123" });
            
            _context.SaveChanges();

            // Mock Authentication Service
            _authServiceMock = new Mock<IAuthenticationService>();
            _authServiceMock.Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);
            _authServiceMock.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProviderMock.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(_authServiceMock.Object);
            _serviceProviderMock.Setup(s => s.GetService(typeof(Microsoft.AspNetCore.Mvc.Routing.IUrlHelperFactory))).Returns(new Mock<Microsoft.AspNetCore.Mvc.Routing.IUrlHelperFactory>().Object);
            _serviceProviderMock.Setup(s => s.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory))).Returns(new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>().Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProviderMock.Object;

            _controller = new AccountController(_context);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Login_Post_ValidCredentials_RedirectsToHomeOrAdmin()
        {
            // Act
            var result = await _controller.Login("admin", "123456");

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            // Admin redirects to Products Index
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Products"));

            // Verify SignIn was called
            _authServiceMock.Verify(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [Test]
        public async Task Login_Post_InvalidCredentials_ReturnsView()
        {
            // Act
            var result = await _controller.Login("admin", "wrongpassword");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Logout_Post_RedirectsToHome()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));

            // Verify SignOut was called
            _authServiceMock.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [Test]
        public async Task Register_Post_ValidUser_RedirectsToHome()
        {
            // Arrange
            var newUser = new User { Username = "newuser", FullName = "New User", Email = "new@test.com", Address = "Addr", Phone = "123" };

            // Act
            var result = await _controller.Register(newUser, "123456", "123456");

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));

            // Verify User was added
            var addedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.That(addedUser, Is.Not.Null);
            Assert.That(addedUser.RoleID, Is.EqualTo(3)); // Customer role
        }

        [Test]
        public async Task Register_Post_DuplicateUsername_ReturnsView()
        {
            // Arrange
            var newUser = new User { Username = "admin", FullName = "New User", Email = "new@test.com", Address = "Addr", Phone = "123" };

            // Act
            var result = await _controller.Register(newUser, "123456", "123456");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }
    }
}
