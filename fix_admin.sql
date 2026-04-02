USE QuanLyDoanKham;
GO
SET NOCOUNT ON;
DELETE FROM Users WHERE Username = 'admin';
INSERT INTO Users (Username, PasswordHash, FullName, RoleId, CreatedAt)
VALUES ('admin', '$2a$11$azxY7U9nnOEV4xF4Cto.gOLsbvUKtALtNoqMgNoWjADuyPMFxlsn0', N'Quản trị viên', 1, GETDATE());
GO
SELECT 'SUCCESS' as Result;
GO
