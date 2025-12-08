# Assignment ASP.NET - Unit Tests

Dự án test này chứa các unit tests cho dự án **Assignment_ASP.NET**, sử dụng **xUnit**, **Moq**, và **Entity Framework Core In-Memory Database**.

## 📋 Tổng quan

Dự án test này kiểm thử **3 controllers đơn giản nhất** trong ứng dụng, mỗi controller có **3 phương thức test** quan trọng:

### 1. **HomeController** (3 tests)
Controller này xử lý trang chủ và hiển thị sản phẩm.

- ✅ **Index_ReturnsViewResult_WithHomeIndexViewModel**
  - Kiểm tra action `Index` trả về view với danh sách sản phẩm và categories
  - Sử dụng Mock để giả lập `IProductService`

- ✅ **Details_WithValidId_ReturnsViewResult**
  - Kiểm tra action `Details` với ID hợp lệ trả về thông tin chi tiết sản phẩm
  - Verify rằng model trả về có đúng ProductID

- ✅ **Promotions_ReturnsViewResult_WithActiveCoupons**
  - Kiểm tra action `Promotions` trả về danh sách coupon đang hoạt động
  - Verify số lượng coupons được trả về

### 2. **CategoriesController** (3 tests)
Controller này quản lý CRUD operations cho Categories.

- ✅ **Details_WithValidId_ReturnsViewResult**
  - Kiểm tra action `Details` với ID hợp lệ trả về thông tin category
  - Sử dụng In-Memory Database để test

- ✅ **Create_Post_WithValidModel_RedirectsToIndex**
  - Kiểm tra việc tạo category mới thành công
  - Verify category được thêm vào database
  - Kiểm tra redirect đến action Index

- ✅ **DeleteConfirmed_WithValidId_RedirectsToIndex**
  - Kiểm tra việc xóa category thành công
  - Verify category bị xóa khỏi database
  - Kiểm tra số lượng categories còn lại

### 3. **CouponsController** (3 tests)
Controller này quản lý CRUD operations cho Coupons (mã giảm giá).

- ✅ **Create_Post_WithValidModel_RedirectsToIndex**
  - Kiểm tra việc tạo coupon mới thành công
  - Verify coupon được thêm vào database với đúng thông tin

- ✅ **Edit_Post_WithValidModel_RedirectsToIndex**
  - Kiểm tra việc cập nhật coupon thành công
  - Verify thông tin coupon được cập nhật đúng trong database

- ✅ **DeleteConfirmed_WithValidId_RedirectsToIndex**
  - Kiểm tra việc xóa coupon thành công
  - Verify coupon bị xóa khỏi database

## 🛠️ Công nghệ sử dụng

- **xUnit** - Framework testing chính
- **Moq** - Library để tạo mock objects (dùng cho HomeController)
- **Entity Framework Core In-Memory Database** - Database giả lập cho testing (dùng cho Categories và Coupons)
- **.NET 9.0** - Framework version

## 📦 Packages

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
```

## 🚀 Cách chạy tests

### Chạy tất cả tests:
```bash
dotnet test Assignment_ASP.NET.Tests
```

### Chạy tests với output chi tiết:
```bash
dotnet test Assignment_ASP.NET.Tests --logger "console;verbosity=detailed"
```

### Chạy tests cho một class cụ thể:
```bash
dotnet test --filter "FullyQualifiedName~HomeControllerTests"
dotnet test --filter "FullyQualifiedName~CategoriesControllerTests"
dotnet test --filter "FullyQualifiedName~CouponsControllerTests"
```

## 📊 Kết quả Test

```
Test Run Successful.
Total tests: 9
     Passed: 9
     Failed: 0
   Skipped: 0
Total time: ~7 seconds
```

## 🎯 Chiến lược Testing

### 1. **Mock-based Testing (HomeController)**
- Sử dụng **Moq** để tạo mock `IProductService`
- Không cần database thực tế
- Test nhanh và độc lập
- Phù hợp cho controllers sử dụng services

### 2. **In-Memory Database Testing (Categories & Coupons)**
- Sử dụng **EF Core In-Memory Database**
- Mỗi test có database riêng (Guid.NewGuid())
- Tự động cleanup sau mỗi test (IDisposable)
- Phù hợp cho CRUD operations

## 📝 Cấu trúc Test

Mỗi test method tuân theo pattern **AAA (Arrange-Act-Assert)**:

```csharp
[Fact]
public async Task TestMethodName()
{
    // Arrange - Chuẩn bị dữ liệu test
    var testData = new TestData();
    
    // Act - Thực hiện action cần test
    var result = await _controller.Action(testData);
    
    // Assert - Kiểm tra kết quả
    Assert.IsType<ExpectedType>(result);
}
```

## 🔍 Lưu ý

1. **In-Memory Database** được tạo mới cho mỗi test class instance
2. **Seed data** được thêm vào trong constructor của test class
3. **Dispose** được gọi tự động sau mỗi test để cleanup
4. Tests hoàn toàn **độc lập** và có thể chạy song song

## 👨‍💻 Tác giả

Dự án test được tạo để kiểm thử các chức năng cơ bản của Assignment ASP.NET.

---

**Lưu ý**: Đây là phiên bản rút gọn với 3 tests cho mỗi controller. Có thể mở rộng thêm các test cases khác như:
- Validation tests
- Error handling tests
- Edge cases tests
- Integration tests
