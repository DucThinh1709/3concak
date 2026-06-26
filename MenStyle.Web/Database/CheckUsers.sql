-- Chạy file này trong SQL Server Management Studio để kiểm tra tài khoản đã đăng ký.
USE MenStyleDb;
GO

SELECT
    Id,
    FullName,
    Email,
    PhoneNumber,
    Address,
    CreatedAt
FROM AspNetUsers
ORDER BY CreatedAt DESC;
GO

-- Kiểm tra vai trò của user
SELECT
    u.Email,
    u.FullName,
    r.Name AS RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.CreatedAt DESC;
GO
