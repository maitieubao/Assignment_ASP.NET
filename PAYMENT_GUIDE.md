# Hướng dẫn sử dụng tính năng thanh toán

## Tổng quan
Hệ thống đã được bổ sung 2 phương thức thanh toán:
1. **COD (Cash on Delivery)** - Thanh toán khi nhận hàng
2. **Chuyển khoản ngân hàng** - Thanh toán giả lập qua ngân hàng

## Các thay đổi đã thực hiện

### 1. Database
- **Bảng Orders** đã được thêm 2 trường mới:
  - `PaymentMethod` (VARCHAR(50)): Lưu phương thức thanh toán ("COD" hoặc "Bank")
  - `PaymentStatus` (VARCHAR(50)): Lưu trạng thái thanh toán ("Pending", "Completed", "Failed")

### 2. Controllers
- **CheckoutController**: 
  - `Index()`: Hiển thị trang checkout với lựa chọn phương thức thanh toán
  - `PlaceOrder()`: Xử lý đặt hàng, phân luồng theo phương thức thanh toán
  - `BankPayment()`: Hiển thị trang thanh toán ngân hàng giả lập
  - `ProcessBankPayment()`: Xử lý thanh toán ngân hàng (giả lập)
  - `OrderConfirmation()`: Hiển thị trang xác nhận đơn hàng

- **MyOrdersController** (Mới):
  - `Index()`: Hiển thị danh sách đơn hàng của customer
  - `Details()`: Hiển thị chi tiết đơn hàng

### 3. Views
- **Checkout/Index.cshtml**: Thêm lựa chọn phương thức thanh toán
- **Checkout/BankPayment.cshtml**: Trang thanh toán ngân hàng giả lập
- **Checkout/OrderConfirmation.cshtml**: Cập nhật hiển thị thông tin thanh toán
- **MyOrders/Index.cshtml**: Danh sách đơn hàng của customer
- **MyOrders/Details.cshtml**: Chi tiết đơn hàng

## Luồng hoạt động

### Thanh toán COD
1. Customer thêm sản phẩm vào giỏ hàng
2. Vào trang Checkout
3. Chọn phương thức "Thanh toán khi nhận hàng (COD)"
4. Nhấn "Đặt hàng"
5. Hệ thống tạo đơn hàng với:
   - `PaymentMethod = "COD"`
   - `PaymentStatus = "Pending"`
   - `Status = "Pending"`
6. Chuyển đến trang OrderConfirmation
7. Hiển thị hướng dẫn thanh toán COD

### Thanh toán ngân hàng
1. Customer thêm sản phẩm vào giỏ hàng
2. Vào trang Checkout
3. Chọn phương thức "Chuyển khoản ngân hàng"
4. Nhấn "Đặt hàng"
5. Hệ thống tạo đơn hàng với:
   - `PaymentMethod = "Bank"`
   - `PaymentStatus = "Pending"`
   - `Status = "Pending"`
6. Chuyển đến trang BankPayment
7. Customer chọn ngân hàng (Vietcombank, VietinBank, BIDV, Agribank, Techcombank, MB Bank)
8. Nhấn "Thanh toán"
9. Hệ thống cập nhật:
   - `PaymentStatus = "Completed"`
   - `Status = "Approved"`
10. Chuyển đến trang OrderConfirmation
11. Hiển thị thông báo thanh toán thành công

## Xem đơn hàng
- Customer có thể xem danh sách đơn hàng tại: `/MyOrders`
- Xem chi tiết đơn hàng tại: `/MyOrders/Details/{id}`
- Mỗi đơn hàng hiển thị:
  - Mã đơn hàng
  - Ngày đặt
  - Trạng thái đơn hàng
  - Phương thức thanh toán
  - Trạng thái thanh toán
  - Tổng tiền
  - Chi tiết sản phẩm

## Lưu ý
- Đây là hệ thống thanh toán **GIẢ LẬP** cho mục đích demo
- Không có giao dịch thực tế nào được thực hiện
- Thanh toán ngân hàng sẽ được xác nhận ngay lập tức
- Admin/Employee có thể quản lý đơn hàng tại `/Orders`
- Customer chỉ có thể xem đơn hàng của chính mình tại `/MyOrders`

## Test
Để test tính năng:
1. Đăng nhập với tài khoản customer (username: `customer`, password: `123456`)
2. Thêm sản phẩm vào giỏ hàng
3. Vào trang Checkout
4. Thử cả 2 phương thức thanh toán
5. Kiểm tra đơn hàng tại "Đơn hàng của tôi"
