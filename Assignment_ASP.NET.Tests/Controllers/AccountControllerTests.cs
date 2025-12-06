using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho AccountController
    /// </summary>
    [TestFixture]
    public class AccountControllerTests : ControllerTestBase
    {
        private AccountController _controller = null!;
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private Mock<IServiceProvider> _serviceProviderMock = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Account";

        protected override void SeedCommonData()
        {
            // Seed roles và admin user
            SeedRoles();
            SeedAdminUser();
        }

        protected override void AdditionalSetup()
        {
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

            _controller = new AccountController(Context);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }

        #region Login Tests

        [Test]
        public async Task Login_Post_ValidCredentials_RedirectsToHomeOrAdmin()
        {
            // Act
            var result = await _controller.Login(TestConstants.AdminUsername, TestConstants.DefaultPassword);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            // Admin redirects to Products Index
            Assert.That(redirectResult!.ActionName, Is.EqualTo(TestConstants.IndexAction));
            Assert.That(redirectResult.ControllerName, Is.EqualTo(TestConstants.ProductsController));

            // Verify SignIn was called
            _authServiceMock.Verify(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [Test]
        public async Task Login_Post_InvalidCredentials_ReturnsView()
        {
            // Act
            var result = await _controller.Login(TestConstants.AdminUsername, "wrongpassword");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False, "ModelState should be invalid for wrong password");
        }

        #endregion

        #region Logout Tests

        [Test]
        public async Task Logout_Post_RedirectsToHome()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo(TestConstants.IndexAction));
            Assert.That(redirectResult.ControllerName, Is.EqualTo(TestConstants.HomeController));

            // Verify SignOut was called
            _authServiceMock.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        #endregion

        #region Register Tests

        [Test]
        public async Task Register_Post_ValidUser_RedirectsToHome()
        {
            // Arrange
            var newUser = new User 
            { 
                Username = "newuser", 
                FullName = "New User", 
                Email = "new@test.com", 
                Address = TestConstants.DefaultAddress, 
                Phone = TestConstants.DefaultPhone 
            };

            // Act
            var result = await _controller.Register(newUser, TestConstants.DefaultPassword, TestConstants.DefaultPassword);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo(TestConstants.IndexAction));
            Assert.That(redirectResult.ControllerName, Is.EqualTo(TestConstants.HomeController));

            // Verify User was added
            var addedUser = await Context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.That(addedUser, Is.Not.Null, "New user should be added to database");
            Assert.That(addedUser!.RoleID, Is.EqualTo(TestConstants.CustomerRoleId), "New user should have Customer role");
        }

        [Test]
        public async Task Register_Post_DuplicateUsername_ReturnsView()
        {
            // Arrange
            var newUser = new User 
            { 
                Username = TestConstants.AdminUsername, 
                FullName = "New User", 
                Email = "new@test.com", 
                Address = TestConstants.DefaultAddress, 
                Phone = TestConstants.DefaultPhone 
            };

            // Act
            var result = await _controller.Register(newUser, TestConstants.DefaultPassword, TestConstants.DefaultPassword);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False, "ModelState should be invalid for duplicate username");
        }

        #endregion
    }
}
