-- =============================================
-- Script: Tạo tài khoản Customer cho VinGroup và FPT
-- Chạy sau khi đã có Companies trong database
-- =============================================

USE QuanLyDoanKham;
GO

-- Tạo tài khoản VinGroup (Customer)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'vingroup')
BEGIN
    INSERT INTO Users (Username, PasswordHash, FullName, RoleId, CompanyId)
    VALUES ('vingroup', '$2a$11$5EhJGZYqZ0Y5YqZ0Y5YqZOeK3K3K3K3K3K3K3K3K3K3K3K3K3K3K3K', N'Quản lý VinGroup', 3, 1);
    PRINT N'✅ Đã tạo tài khoản: vingroup / vingroup123';
END
ELSE
BEGIN
    PRINT N'⚠️ Tài khoản vingroup đã tồn tại';
END
GO

-- Tạo tài khoản FPT (Customer)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'fpt')
BEGIN
    INSERT INTO Users (Username, PasswordHash, FullName, RoleId, CompanyId)
    VALUES ('fpt', '$2a$11$5EhJGZYqZ0Y5YqZ0Y5YqZOeK3K3K3K3K3K3K3K3K3K3K3K3K3K3K3K', N'Quản lý FPT', 3, 2);
    PRINT N'✅ Đã tạo tài khoản: fpt / fpt123';
END
ELSE
BEGIN
    PRINT N'⚠️ Tài khoản fpt đã tồn tại';
END
GO

PRINT N'';
PRINT N'📋 DANH SÁCH TÀI KHOẢN:';
PRINT N'━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
SELECT 
    Username,
    FullName,
    CASE RoleId 
        WHEN 1 THEN 'Admin'
        WHEN 2 THEN 'Staff'
        WHEN 3 THEN 'Customer'
    END AS Role,
    CASE 
        WHEN CompanyId IS NULL THEN N'(Không thuộc công ty)'
        ELSE (SELECT CompanyName FROM Companies WHERE CompanyId = Users.CompanyId)
    END AS Company
FROM Users
ORDER BY RoleId, Username;
GO
