# Khắc Phục Lỗi "Không tìm thấy website" VNPay

## ✅ Đã Sửa:

### 1. **URL VNPay sai**
- ❌ Cũ: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`
- ✅ Mới: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.htm`

### 2. **TxnRef sử dụng OrderID**
- ❌ Cũ: Dùng `DateTime.Now.Ticks` (số quá dài, khó track)
- ✅ Mới: Dùng `order.OrderID` (dễ track và map với database)

## 🔍 Nguyên nhân lỗi:

Lỗi **"Không tìm thấy website"** xảy ra khi:
1. URL endpoint sai (`.html` thay vì `.htm`)
2. TmnCode không đúng
3. Signature không hợp lệ
4. ReturnUrl không được VNPay chấp nhận

## 📋 Checklist để test lại:

### Trên Local (Development):
```bash
# 1. Build lại project
dotnet build

# 2. Chạy ứng dụng
dotnet run

# 3. Test thanh toán với thông tin:
# - Ngân hàng: NCB
# - Số thẻ: 9704198526191432198
# - Tên: NGUYEN VAN A
# - Ngày: 07/15
# - OTP: 123456
```

### Trên Render (Production):
```bash
# 1. Commit và push
git add .
git commit -m "Fix VNPay URL and use OrderID for TxnRef"
git push origin main

# 2. Đợi Render deploy xong

# 3. Kiểm tra Environment Variables:
# VnPay__ReturnUrl=https://assignment-asp-net.onrender.com/Checkout/VnPayReturn
```

## 🎯 Điểm quan trọng:

### ReturnUrl phải:
- ✅ Sử dụng HTTPS (không phải HTTP)
- ✅ Là domain thật (không phải localhost khi production)
- ✅ Endpoint có `[AllowAnonymous]`
- ✅ Không có trailing slash

### Ví dụ đúng:
```
https://assignment-asp-net.onrender.com/Checkout/VnPayReturn
```

### Ví dụ SAI:
```
http://assignment-asp-net.onrender.com/Checkout/VnPayReturn/
https://localhost:5215/Checkout/VnPayReturn (chỉ dùng local)
```

## 🧪 Test Flow:

1. **Tạo đơn hàng** → Chọn VNPay
2. **Redirect đến VNPay** → Nhập thông tin thẻ test
3. **VNPay xử lý** → Redirect về ReturnUrl
4. **Callback xử lý** → Verify signature
5. **Update order** → Hiển thị kết quả

## 🔐 Thông tin Test VNPay Sandbox:

```
TmnCode: 2QXUI1L5
HashSecret: AELPHGNYYQZTSNGRBWHKOWJTDGCNJIXS

Thẻ test:
- Ngân hàng: NCB
- Số thẻ: 9704198526191432198
- Tên chủ thẻ: NGUYEN VAN A
- Ngày phát hành: 07/15
- Mật khẩu OTP: 123456
```

## 📝 Nếu vẫn lỗi:

1. Kiểm tra log trên Render
2. Verify signature có đúng không
3. Check ReturnUrl có accessible không
4. Đảm bảo HTTPS được enable
5. Test với Postman/curl để debug

## 🚀 Deploy:

```bash
git add .
git commit -m "Fix VNPay integration - correct URL and TxnRef"
git push origin main
```

Sau khi deploy, test lại với thẻ sandbox ở trên.
