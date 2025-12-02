using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Models;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Assignment_ASP.NET.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho UsersController
    /// </summary>
    [TestFixture]
    public class UsersControllerTests : ControllerTestBase
    {
        private UsersController _controller = null!;

        protected override string DatabaseNamePrefix => "TestDatabase_Users";

        protected override void SeedCommonData()
        {
            // Seed roles và admin user
            SeedRoles();
            SeedAdminUser();
        }

        protected override void AdditionalSetup()
        {
            _controller = new UsersController(Context);
        }

        [TearDown]
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }

        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithUsers()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as List<User>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(1), "Should return 1 user (admin)");
        }

        #endregion

        #region Create Tests

        [Test]
        public async Task Create_Post_ValidUser_RedirectsToIndex()
        {
            // Arrange
            var newUser = new User 
            { 
                Username = "newuser", 
                FullName = "New User", 
                Email = "new@test.com", 
                Address = TestConstants.DefaultAddress, 
                Phone = TestConstants.DefaultPhone, 
                RoleID = TestConstants.StaffRoleId 
            };

            // Act
            var result = await _controller.Create(newUser, "password");

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo(TestConstants.IndexAction));

            // Verify User added and password hashed
            var addedUser = await Context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.That(addedUser, Is.Not.Null, "New user should be added");
            Assert.That(addedUser!.PasswordHash, Is.Not.Null, "Password should be hashed");
            Assert.That(addedUser.PasswordHash, Is.Not.EqualTo("password"), "Password should not be stored in plain text");
        }

        #endregion

        #region Edit Tests

        [Test]
        public async Task Edit_Post_UpdatesUser_And_Password()
        {
            // Arrange
            var user = Context.Users.First();
            var originalName = user.FullName;
            user.FullName = "Updated Name";
            const string newPassword = "newpassword";

            // Act
            var result = await _controller.Edit(user.UserID, user, newPassword);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            
            var updatedUser = await Context.Users.FindAsync(user.UserID);
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser!.FullName, Is.EqualTo("Updated Name"));
            Assert.That(updatedUser.FullName, Is.Not.EqualTo(originalName), "Name should be updated");
            
            // Verify password changed
            var expectedHash = TestDataBuilder.HashPassword(newPassword);
            Assert.That(updatedUser.PasswordHash, Is.EqualTo(expectedHash), "Password should be updated and hashed");
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task DeleteConfirmed_Fails_IfUserHasOrders()
        {
            // Arrange
            var user = Context.Users.First();
            Context.Orders.Add(new Order 
            { 
                OrderID = 1, 
                UserID = user.UserID, 
                OrderDate = DateTime.Now, 
                Status = "Pending", 
                TotalAmount = 100, 
                ShippingAddress = TestConstants.DefaultAddress 
            });
            await Context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(user.UserID);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Should return view when user has orders");
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("Delete"));
            Assert.That(_controller.ModelState.IsValid, Is.False, "Cannot delete user with orders");
        }

        [Test]
        public async Task DeleteConfirmed_Succeeds_IfNoOrders()
        {
            // Arrange
            var initialCount = Context.Users.Count();

            // Act
            var result = await _controller.DeleteConfirmed(TestConstants.AdminUserId);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(Context.Users.Count(), Is.EqualTo(initialCount - 1), "Should remove user");
            
            var deletedUser = await Context.Users.FindAsync(TestConstants.AdminUserId);
            Assert.That(deletedUser, Is.Null, "Deleted user should not exist");
        }

        #endregion
    }
}
