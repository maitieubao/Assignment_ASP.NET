using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Assignment_ASP.NET.Tests.Base
{
    /// <summary>
    /// Base class cho tất cả controller tests
    /// Cung cấp setup chung cho DbContext và các utilities
    /// </summary>
    public abstract class ControllerTestBase
    {
        protected ApplicationDbContext Context { get; private set; }

        /// <summary>
        /// Tên database prefix cho mỗi test class
        /// Override trong derived class để tạo unique database name
        /// </summary>
        protected abstract string DatabaseNamePrefix { get; }

        /// <summary>
        /// Setup được gọi trước mỗi test
        /// </summary>
        [SetUp]
        public virtual void BaseSetUp()
        {
            // Tạo unique database cho mỗi test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"{DatabaseNamePrefix}_{Guid.NewGuid()}")
                .Options;

            Context = new ApplicationDbContext(options);

            // Seed common data nếu cần
            SeedCommonData();

            // Gọi setup riêng của từng test class
            AdditionalSetup();
        }

        /// <summary>
        /// Cleanup được gọi sau mỗi test
        /// </summary>
        [TearDown]
        public virtual void BaseTearDown()
        {
            // Cleanup riêng của từng test class
            AdditionalTearDown();

            // Cleanup database
            Context?.Database.EnsureDeleted();
            Context?.Dispose();
        }

        /// <summary>
        /// Seed data chung cho tất cả tests
        /// Override nếu cần seed data khác
        /// </summary>
        protected virtual void SeedCommonData()
        {
            // Mặc định không seed gì
            // Derived classes có thể override để seed data
        }

        /// <summary>
        /// Setup bổ sung cho từng test class
        /// Override trong derived class nếu cần
        /// </summary>
        protected virtual void AdditionalSetup()
        {
            // Mặc định không làm gì
        }

        /// <summary>
        /// Cleanup bổ sung cho từng test class
        /// Override trong derived class nếu cần
        /// </summary>
        protected virtual void AdditionalTearDown()
        {
            // Mặc định không làm gì
        }

        #region Helper Methods

        /// <summary>
        /// Seed roles vào database
        /// </summary>
        protected void SeedRoles()
        {
            var roles = TestDataBuilder.CreateDefaultRoles();
            Context.Roles.AddRange(roles);
            Context.SaveChanges();
        }

        /// <summary>
        /// Seed categories vào database
        /// </summary>
        protected void SeedCategories()
        {
            var categories = TestDataBuilder.CreateDefaultCategories();
            Context.Categories.AddRange(categories);
            Context.SaveChanges();
        }

        /// <summary>
        /// Seed products vào database
        /// </summary>
        protected void SeedProducts()
        {
            var products = TestDataBuilder.CreateDefaultProducts();
            Context.Products.AddRange(products);
            Context.SaveChanges();
        }

        /// <summary>
        /// Seed default user vào database
        /// </summary>
        protected void SeedDefaultUser()
        {
            var user = TestDataBuilder.CreateDefaultUser();
            Context.Users.Add(user);
            Context.SaveChanges();
        }

        /// <summary>
        /// Seed admin user vào database
        /// </summary>
        protected void SeedAdminUser()
        {
            var user = TestDataBuilder.CreateAdminUser();
            Context.Users.Add(user);
            Context.SaveChanges();
        }

        #endregion
    }
}
