# ✅ Đã Sửa Lỗi 404 VNPay Callback

## 🔍 Nguyên nhân:

Bạn đang test trên **localhost** nhưng `ReturnUrl` trong `appsettings.json` đang trỏ đến **Render production**:
```
https://assignment-asp-net.onrender.com/Checkout/VnPayReturn
```

Khi VNPay redirect về, nó sẽ redirect đến Render chứ không phải localhost → Lỗi 404!

## ✅ Giải pháp:

Đã tạo file `appsettings.Development.json` để override ReturnUrl cho môi trường local:

```json
{
  "VnPay": {
    "ReturnUrl": "http://localhost:5215/Checkout/VnPayReturn"
  }
}
```

## 🚀 Cách test:

### 1. **Restart ứng dụng** (đã chạy rồi)
```bash
# Nếu chưa chạy:
dotnet run
```

### 2. **Mở trình duyệt**
```
http://localhost:5215
```

### 3. **Thực hiện thanh toán**
- Đăng nhập (hoặc đăng ký)
- Thêm sản phẩm vào giỏ
- Checkout → Chọn **VNPay**
- Nhập thông tin thẻ test:

```
Ngân hàng: NCB
Số thẻ: 9704198526191432198
Tên chủ thẻ: NGUYEN VAN A
Ngày phát hành: 07/15
Mật khẩu OTP: 123456
```

### 4. **Kết quả mong đợi**
- VNPay xử lý thanh toán
- Redirect về: `http://localhost:5215/Checkout/VnPayReturn?vnp_...`
- Hiển thị trang OrderConfirmation với trạng thái "Thanh toán thành công"

## 📋 Cấu trúc Config:

### `appsettings.json` (Production/Default)
```json
{
  "VnPay": {
    "ReturnUrl": "https://assignment-asp-net.onrender.com/Checkout/VnPayReturn"
  }
}
```

### `appsettings.Development.json` (Local)
```json
{
  "VnPay": {
    "ReturnUrl": "http://localhost:5215/Checkout/VnPayReturn"
  }
}
```

ASP.NET Core tự động merge configs:
- Development: Dùng localhost
- Production (Render): Dùng domain thật

## 🔐 Environment Variables trên Render:

Trên Render, bạn có thể override bằng Environment Variable:
```
VnPay__ReturnUrl=https://assignment-asp-net.onrender.com/Checkout/VnPayReturn
```

## ⚠️ Lưu ý:

1. **Local**: Dùng `http://localhost:5215` (không cần HTTPS)
2. **Production**: Phải dùng `https://` (VNPay yêu cầu)
3. **Không trailing slash**: `/VnPayReturn` ✅, `/VnPayReturn/` ❌
4. **Endpoint phải có `[AllowAnonymous]`**: Đã có rồi ✅

## 🧪 Debug nếu vẫn lỗi:

### Kiểm tra URL VNPay tạo ra:
Thêm log trong `VnPayService.cs`:
```csharp
var paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:Url"], _config["VnPay:HashSecret"]);
Console.WriteLine($"VNPay URL: {paymentUrl}");
return paymentUrl;
```

### Kiểm tra callback params:
Thêm log trong `VnPayReturn`:
```csharp
public async Task<IActionResult> VnPayReturn()
{
    Console.WriteLine($"Callback URL: {Request.QueryString}");
    var response = _vnPayService.ProcessCallback(Request.Query);
    // ...
}
```

## 📝 Checklist:

- [x] Tạo `appsettings.Development.json`
- [x] ReturnUrl trỏ đến localhost
- [x] Endpoint có `[AllowAnonymous]`
- [x] Ứng dụng đang chạy trên port 5215
- [ ] Test thanh toán với thẻ sandbox
- [ ] Verify callback thành công

Bây giờ hãy test lại!
