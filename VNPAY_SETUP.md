# Hướng Dẫn Cấu Hình VNPay Hoàn Chỉnh

## 1. Cấu hình trên Render.com

Vào **Environment Variables** của service trên Render và thêm:

```
VnPay__ReturnUrl=https://assignment-asp-net.onrender.com/Checkout/VnPayReturn
```

**Lưu ý:** Thay `assignment-asp-net` bằng tên app thực tế của bạn trên Render.

## 2. Cấu hình Local (Development)

File `appsettings.Development.json`:

```json
{
  "VnPay": {
    "ReturnUrl": "http://localhost:5215/Checkout/VnPayReturn"
  }
}
```

## 3. Thông Tin VNPay Sandbox

- **TmnCode:** 2QXUI1L5
- **HashSecret:** AELPHGNYYQZTSNGRBWHKOWJTDGCNJIXS
- **URL:** https://sandbox.vnpayment.vn/paymentv2/vpcpay.html

## 4. Luồng Hoạt Động

1. **Khách hàng chọn VNPay** → Hệ thống tạo URL thanh toán
2. **Redirect đến VNPay** → Khách hàng nhập thông tin thẻ
3. **VNPay xử lý** → Redirect về `/Checkout/VnPayReturn`
4. **Hệ thống xác thực** → Kiểm tra chữ ký HMACSHA512
5. **Cập nhật đơn hàng** → Hiển thị kết quả

## 5. Test VNPay Sandbox

Sử dụng thông tin thẻ test:

- **Ngân hàng:** NCB
- **Số thẻ:** 9704198526191432198
- **Tên chủ thẻ:** NGUYEN VAN A
- **Ngày phát hành:** 07/15
- **Mật khẩu OTP:** 123456

## 6. Các Endpoint

- **Tạo thanh toán:** `POST /Checkout/PlaceOrder` (paymentMethod=VnPay)
- **Callback VNPay:** `GET /Checkout/VnPayReturn` (AllowAnonymous)
- **Xác nhận đơn:** `GET /Checkout/OrderConfirmation/{orderId}`

## 7. Response Codes VNPay

- **00:** Giao dịch thành công
- **07:** Trừ tiền thành công, giao dịch bị nghi ngờ
- **09:** Thẻ chưa đăng ký dịch vụ
- **10:** Xác thực thông tin thẻ sai quá số lần
- **11:** Hết hạn chờ thanh toán
- **24:** Khách hàng hủy giao dịch
- **51:** Tài khoản không đủ số dư

## 8. Troubleshooting

### Lỗi: Invalid Signature
- Kiểm tra `HashSecret` đúng chưa
- Đảm bảo không có khoảng trắng thừa
- Kiểm tra encoding UTF-8

### Lỗi: 404 Not Found khi callback
- Kiểm tra `ReturnUrl` trong Environment Variables
- Đảm bảo endpoint có `[AllowAnonymous]`
- Kiểm tra routing đúng chưa

### Lỗi: Không tìm thấy đơn hàng
- VNPay trả về `vnp_TxnRef` (timestamp)
- Cần map lại với OrderID trong database
- Hiện tại đang dùng timestamp làm TxnRef

## 9. Deploy Checklist

- [ ] Push code lên GitHub
- [ ] Cấu hình Environment Variable `VnPay__ReturnUrl`
- [ ] Deploy trên Render
- [ ] Test thanh toán với thẻ sandbox
- [ ] Kiểm tra log nếu có lỗi

## 10. Bảo Mật

- ✅ HMACSHA512 cho chữ ký
- ✅ Validate signature từ VNPay
- ✅ AllowAnonymous chỉ cho callback endpoint
- ✅ Kiểm tra quyền truy cập đơn hàng
- ✅ HTTPS bắt buộc trên production
