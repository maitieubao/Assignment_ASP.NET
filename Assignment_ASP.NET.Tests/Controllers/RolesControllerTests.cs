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
    /// Unit tests cho RolesController
    /// </summary>
    [TestFixture]
    public class RolesControllerTests : ControllerTestBase
    {
        private RolesController _controller = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Roles";

        protected override void SeedCommonData()
        {
            // Seed roles
            SeedRoles();
        }

        protected override void AdditionalSetup()
        {
            _controller = new RolesController(Context);
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }

        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithRoles()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as List<Role>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(3), "Should return 3 default roles");
        }

        #endregion

        #region Create Tests

        [Test]
        public async Task Create_Post_ValidRole_RedirectsToIndex()
        {
            // Arrange
            var role = new Role { RoleName = "Manager" };
            var initialCount = Context.Roles.Count();

            // Act
            var result = await _controller.Create(role);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(Context.Roles.Count(), Is.EqualTo(initialCount + 1), "Should add new role");
        }

        [Test]
        public async Task Create_Post_DuplicateRoleName_ReturnsView()
        {
            // Arrange
            var role = new Role { RoleName = TestConstants.AdminRoleName };

            // Act
            var result = await _controller.Create(role);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False, "ModelState should be invalid for duplicate role name");
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task DeleteConfirmed_Fails_IfRoleHasUsers()
        {
            // Arrange
            var role = Context.Roles.First();
            Context.Users.Add(TestDataBuilder.CreateUser(
                userId: 10,
                username: "testuser",
                email: "test@test.com",
                roleId: role.RoleID
            ));
            await Context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(role.RoleID);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("Delete"));
            Assert.That(_controller.ModelState.IsValid, Is.False, "Cannot delete role with users");
        }

        [Test]
        public async Task DeleteConfirmed_Succeeds_IfNoUsers()
        {
            // Arrange
            var initialCount = Context.Roles.Count();

            // Act
            var result = await _controller.DeleteConfirmed(TestConstants.AdminRoleId);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(Context.Roles.Count(), Is.EqualTo(initialCount - 1), "Should remove role");
        }

        #endregion
    }
}
