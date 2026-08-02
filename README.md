## Database

Project sử dụng SQL Server + Entity Framework Core Migration.

Khi chạy project, hãy chạy lần lượt các lệnh sau trên terminal

- cd .\MenStyle.Web
- dotnet ef database update

## Trung tâm quản trị

- Đường dẫn quản trị: `/Admin`
- Chỉ tài khoản thuộc role `Admin` mới có quyền truy cập.
- Sau khi admin đăng nhập, thanh điều hướng chỉ hiển thị một lối vào `Quản trị`.
- Trang quản trị tập trung thống kê, quản lý sản phẩm và xử lý đơn hàng.
