# MenStyleMvcLoginSql - ASP.NET Core MVC .NET 8

Project demo website bán quần áo nam MENSTYLE có đăng ký, đăng nhập, đăng xuất và lưu tài khoản vào SQL Server bằng ASP.NET Core Identity + Entity Framework Core.

## Tài khoản demo admin

- Email: admin@menstyle.vn
- Password: Admin@123

Tài khoản admin được tạo tự động lần đầu khi chạy project.

## Cách chạy bằng Visual Studio

1. Giải nén file zip.
2. Mở `MenStyleMvcDemo.sln`.
3. Chọn project `MenStyle.Web` làm Startup Project.
4. Kiểm tra connection string trong `MenStyle.Web/appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MenStyleDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

5. Bấm `Ctrl + F5` hoặc nút Run.
6. Vào `/Account/Register` để đăng ký tài khoản mới.
7. Vào `/Account/Login` để đăng nhập.
8. Vào `/Account/Profile` để xem thông tin được đọc từ database.

## Nếu máy không có LocalDB

Nếu bạn dùng SQL Server Express hoặc SQL Server bình thường, đổi connection string thành ví dụ:

```json
"DefaultConnection": "Server=.;Database=MenStyleDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

hoặc:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=MenStyleDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## Database được tạo như thế nào?

Trong `Program.cs`, project gọi:

```csharp
await DbInitializer.SeedAsync(app.Services);
```

Trong `DbInitializer.cs`, lệnh `EnsureCreatedAsync()` sẽ tự tạo database `MenStyleDb` và các bảng Identity nếu database chưa có.

Các bảng quan trọng:

- AspNetUsers: lưu tài khoản người dùng
- AspNetRoles: lưu vai trò Admin / Customer
- AspNetUserRoles: liên kết user với role

## Kiểm tra dữ liệu trong SQL Server

Mở SQL Server Management Studio, chọn database `MenStyleDb`, chạy file:

```text
MenStyle.Web/Database/CheckUsers.sql
```

## Các file quan trọng đã thêm

```text
Data/ApplicationDbContext.cs
Data/DbInitializer.cs
Models/ApplicationUser.cs
Controllers/AccountController.cs
ViewModels/Auth/RegisterViewModel.cs
ViewModels/Auth/LoginViewModel.cs
Views/Account/Register.cshtml
Views/Account/Login.cshtml
Views/Account/Profile.cshtml
Views/Account/AccessDenied.cshtml
Database/CheckUsers.sql
```

## Ghi chú

Bản này đang dùng ASP.NET Core Identity để làm đăng nhập/đăng ký chuẩn hơn so với tự tạo bảng User thủ công. Khi đăng ký tài khoản, mật khẩu sẽ được hash trước khi lưu vào SQL Server, không lưu mật khẩu dạng chữ thường.
