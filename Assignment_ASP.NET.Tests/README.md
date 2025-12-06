# Assignment ASP.NET Tests - Hướng Dẫn

## 📁 Cấu Trúc Dự Án

```
Assignment_ASP.NET.Tests/
├── Base/
│   └── ControllerTestBase.cs          # Base class cho tất cả controller tests
├── Controllers/
│   ├── AccountControllerTests.cs      # Tests cho Account controller
│   ├── CartControllerTests.cs         # Tests cho Cart controller
│   ├── CategoriesControllerTests.cs   # Tests cho Categories controller
│   ├── CheckoutControllerTests.cs     # Tests cho Checkout controller
│   ├── HomeControllerTests.cs         # Tests cho Home controller
│   ├── OrdersControllerTests.cs       # Tests cho Orders controller
│   ├── ProductsControllerTests.cs     # Tests cho Products controller
│   ├── RolesControllerTests.cs        # Tests cho Roles controller
│   └── UsersControllerTests.cs        # Tests cho Users controller
└── Helpers/
    ├── SessionHelper.cs               # Helper methods cho Session operations
    ├── TestConstants.cs               # Tập trung các hằng số test
    └── TestDataBuilder.cs             # Builder pattern để tạo test data

```

## 🎯 Các Thành Phần Chính

### 1. ControllerTestBase (Base Class)

**Mục đích**: Tránh code lặp lại trong setup/teardown và cung cấp helper methods chung.

**Cách sử dụng**:
```csharp
public class MyControllerTests : ControllerTestBase
{
    private MyController _controller;
    
    // Định nghĩa tên database prefix
    protected override string DatabaseNamePrefix => "TestDatabase_MyController";
    
    // Seed data chung cho tất cả tests
    protected override void SeedCommonData()
    {
        SeedCategories();
        SeedProducts();
    }
    
    // Setup riêng cho controller
    protected override void AdditionalSetup()
    {
        _controller = new MyController(Context);
    }
    
    // Cleanup riêng
    protected override void AdditionalTearDown()
    {
        _controller?.Dispose();
    }
}
```

**Helper methods có sẵn**:
- `SeedRoles()` - Seed default roles
- `SeedCategories()` - Seed default categories
- `SeedProducts()` - Seed default products
- `SeedDefaultUser()` - Seed default user
- `SeedAdminUser()` - Seed admin user

### 2. TestConstants

**Mục đích**: Tập trung tất cả các hằng số test ở một nơi, tránh magic values.

**Ví dụ sử dụng**:
```csharp
// Thay vì:
var user = new User { UserID = 1, Username = "testuser" };

// Sử dụng:
var user = new User 
{ 
    UserID = TestConstants.DefaultUserId, 
    Username = TestConstants.DefaultUsername 
};
```

**Các nhóm constants**:
- User & Admin constants
- Role constants
- Category constants
- Product constants
- Controller & Action names

### 3. TestDataBuilder

**Mục đích**: Tạo test data dễ dàng và nhất quán với Builder pattern.

**Ví dụ sử dụng**:
```csharp
// Tạo default user
var user = TestDataBuilder.CreateDefaultUser();

// Tạo admin user
var admin = TestDataBuilder.CreateAdminUser();

// Tạo custom user
var customUser = TestDataBuilder.CreateUser(2, "john", "john@test.com", TestConstants.CustomerRoleId);

// Tạo default categories
var categories = TestDataBuilder.CreateDefaultCategories();

// Tạo default products
var products = TestDataBuilder.CreateDefaultProducts();

// Tạo cart item
var cartItem = TestDataBuilder.CreateCartItem(productId: 1, quantity: 2, price: 100m);
```

### 4. SessionHelper

**Mục đích**: Đơn giản hóa việc làm việc với Session trong tests.

**Ví dụ sử dụng**:
```csharp
// Setup empty cart
SessionHelper.SetupEmptyCart(_mockSession);

// Setup cart với items
var cartItems = TestDataBuilder.CreateDefaultCartItems();
SessionHelper.SetupCartWithItems(_mockSession, cartItems);

// Verify cart được set
SessionHelper.VerifyCartSet(_mockSession, Times.Once());

// Verify cart được remove
SessionHelper.VerifyCartRemoved(_mockSession, Times.Once());

// Verify cart có số lượng items cụ thể
SessionHelper.VerifyCartSetWithItemCount(_mockSession, expectedCount: 2);

// Verify cart có quantity cụ thể cho product
SessionHelper.VerifyCartSetWithQuantity(_mockSession, productId: 1, expectedQuantity: 5);
```

## 📝 Best Practices

### 1. Tổ Chức Tests với Regions

Sử dụng `#region` để nhóm các tests liên quan:

```csharp
#region Index Tests

[Test]
public async Task Index_ReturnsViewResult_WithCategories()
{
    // Test implementation
}

#endregion

#region Create Tests

[Test]
public async Task Create_Post_ValidCategory_RedirectsToIndex()
{
    // Test implementation
}

#endregion
```

### 2. Naming Convention

**Test methods**: `MethodName_Scenario_ExpectedResult`

Ví dụ:
- `Index_ReturnsViewResult_WithCategories`
- `Create_Post_ValidCategory_RedirectsToIndex`
- `Delete_RemovesCategory_WhenExists`

### 3. Arrange-Act-Assert Pattern

Luôn sử dụng AAA pattern rõ ràng:

```csharp
[Test]
public async Task Create_Post_ValidCategory_RedirectsToIndex()
{
    // Arrange
    var category = new Category { CategoryName = "Tablet" };

    // Act
    var result = await _controller.Create(category);

    // Assert
    Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    Assert.That(Context.Categories.Count(), Is.EqualTo(3));
}
```

### 4. Meaningful Assertions

Thêm message cho assertions để dễ debug:

```csharp
Assert.That(model.Count, Is.EqualTo(2), "Should return 2 categories");
Assert.That(Context.Orders.Count(), Is.EqualTo(0), "Should not create order when cart is empty");
```

### 5. Sử dụng Constants

Luôn sử dụng constants thay vì hard-coded values:

```csharp
// ❌ Không tốt
var result = await _controller.Details(1);
Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

// ✅ Tốt
var result = await _controller.Details(TestConstants.PhoneCategoryId);
Assert.That(redirectResult.ActionName, Is.EqualTo(TestConstants.IndexAction));
```

## 🚀 Chạy Tests

### Chạy tất cả tests:
```bash
dotnet test
```

### Chạy tests cho một class cụ thể:
```bash
dotnet test --filter "FullyQualifiedName~CategoriesControllerTests"
```

### Chạy một test method cụ thể:
```bash
dotnet test --filter "FullyQualifiedName~CategoriesControllerTests.Index_ReturnsViewResult_WithCategories"
```

## 🔧 Thêm Test Mới

### Bước 1: Tạo test class kế thừa từ ControllerTestBase

```csharp
using NUnit.Framework;
using Assignment_ASP.NET.Controllers;
using Assignment_ASP.NET.Tests.Base;
using Assignment_ASP.NET.Tests.Helpers;

namespace Assignment_ASP.NET.Tests.Controllers
{
    [TestFixture]
    public class MyControllerTests : ControllerTestBase
    {
        private MyController _controller;
        
        protected override string DatabaseNamePrefix => "TestDatabase_MyController";
        
        protected override void AdditionalSetup()
        {
            _controller = new MyController(Context);
        }
        
        protected override void AdditionalTearDown()
        {
            _controller?.Dispose();
        }
    }
}
```

### Bước 2: Thêm test methods

```csharp
#region Index Tests

[Test]
public async Task Index_ReturnsViewResult_WithData()
{
    // Arrange
    SeedProducts(); // Sử dụng helper từ base class
    
    // Act
    var result = await _controller.Index();
    
    // Assert
    Assert.That(result, Is.InstanceOf<ViewResult>());
}

#endregion
```

## 📊 Lợi Ích Của Cấu Trúc Mới

### ✅ Trước Refactoring
- ❌ Code lặp lại nhiều (setup/teardown giống nhau)
- ❌ Magic values nằm rải rác
- ❌ Khó maintain khi thay đổi cấu trúc DB
- ❌ Khó đọc và hiểu
- ❌ Mỗi test file ~150-200 dòng

### ✅ Sau Refactoring
- ✅ Code DRY (Don't Repeat Yourself)
- ✅ Constants tập trung, dễ thay đổi
- ✅ Dễ maintain và mở rộng
- ✅ Rõ ràng, dễ đọc
- ✅ Mỗi test file ~100-120 dòng
- ✅ Tái sử dụng code cao
- ✅ Test data nhất quán

## 🎓 Ví Dụ So Sánh

### Trước:
```csharp
[SetUp]
public void Setup()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase_Categories_" + System.Guid.NewGuid())
        .Options;
    _context = new ApplicationDbContext(options);
    _context.Categories.Add(new Category { CategoryID = 1, CategoryName = "Phone" });
    _context.Categories.Add(new Category { CategoryID = 2, CategoryName = "Laptop" });
    _context.SaveChanges();
    _controller = new CategoriesController(_context);
}

[Test]
public async Task Details_ReturnsViewResult_WithCategory()
{
    var result = await _controller.Details(1);
    Assert.That(result, Is.InstanceOf<ViewResult>());
    var viewResult = result as ViewResult;
    var model = viewResult.Model as Category;
    Assert.That(model.CategoryID, Is.EqualTo(1));
}
```

### Sau:
```csharp
protected override string DatabaseNamePrefix => "TestDatabase_Categories";

protected override void SeedCommonData()
{
    SeedCategories(); // Một dòng thay vì nhiều dòng
}

protected override void AdditionalSetup()
{
    _controller = new CategoriesController(Context);
}

[Test]
public async Task Details_ReturnsViewResult_WithCategory()
{
    // Act
    var result = await _controller.Details(TestConstants.PhoneCategoryId);

    // Assert
    Assert.That(result, Is.InstanceOf<ViewResult>());
    var viewResult = result as ViewResult;
    var model = viewResult.Model as Category;
    Assert.That(model.CategoryID, Is.EqualTo(TestConstants.PhoneCategoryId));
    Assert.That(model.CategoryName, Is.EqualTo(TestConstants.PhoneCategoryName));
}
```

## 📚 Tài Liệu Tham Khảo

- [NUnit Documentation](https://docs.nunit.org/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
