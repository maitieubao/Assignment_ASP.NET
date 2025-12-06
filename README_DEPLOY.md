# Hướng Dẫn Deploy Lên Render.com

Dự án đã được cấu hình để deploy dễ dàng bằng Docker. Tôi đã chuyển cơ sở dữ liệu sang **SQLite** để bạn có thể chạy ngay mà không cần cấu hình SQL Server phức tạp.

## Bước 1: Đẩy code lên GitHub
Đảm bảo bạn đã commit và push toàn bộ code (bao gồm file `Dockerfile` và `.dockerignore` mới tạo) lên repository GitHub của bạn.

## Bước 2: Tạo Web Service trên Render
1. Truy cập [dashboard.render.com](https://dashboard.render.com/).
2. Chọn **New +** -> **Web Service**.
3. Kết nối với repository GitHub của bạn.
4. Điền các thông tin:
   - **Name**: Tên ứng dụng của bạn (ví dụ: `my-aspnet-shop`).
   - **Region**: Singapore (cho tốc độ tốt nhất ở VN).
   - **Branch**: `main` (hoặc branch bạn đang dùng).
   - **Runtime**: Chọn **Docker**.
   - **Instance Type**: Chọn **Free**.

## Bước 3: Cấu hình Environment Variables (Biến môi trường)
Kéo xuống phần **Environment Variables** và thêm các biến sau:

| Key | Value |
| --- | --- |
| `ASPNETCORE_HTTP_PORTS` | `8080` |
| `VnPay__PaymentBackReturnUrl` | `https://<TÊN-APP-CỦA-BẠN>.onrender.com/Checkout/PaymentCallback` |

*Lưu ý: Thay `<TÊN-APP-CỦA-BẠN>` bằng tên bạn đã đặt ở bước 2.*

## Bước 4: Deploy
Nhấn **Create Web Service**. Render sẽ tự động build Docker image và deploy. Quá trình này có thể mất vài phút.

## Lưu ý quan trọng về Database
Dự án đang sử dụng **SQLite** (`app.db`).
- **Ưu điểm**: Deploy thành công ngay lập tức, không cần tạo database riêng.
- **Nhược điểm trên gói Free**: Dữ liệu sẽ bị **reset** mỗi khi server khởi động lại hoặc deploy mới (do Render Free không lưu trữ file hệ thống lâu dài).
- Nếu bạn cần lưu dữ liệu lâu dài, bạn cần sử dụng dịch vụ PostgreSQL rời (Render có cung cấp gói PostgreSQL Free 90 ngày).

Chúc bạn thành công!
