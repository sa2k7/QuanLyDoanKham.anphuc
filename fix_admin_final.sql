USE QuanLyDoanKham;
GO
SET NOCOUNT ON;
DELETE FROM Users WHERE Username = 'admin';
INSERT INTO Users (Username, PasswordHash, FullName, RoleId)
VALUES ('admin', '$2a$11$jOsS3wMAuwX6/FpdXFGW7ehkvp0hmY1iQTXSCKbLR91UzwFsulEZ2', N'Quản trị viên', 1);
GO
SELECT Username, LEN(PasswordHash) as HashLength FROM Users WHERE Username = 'admin';
GO
