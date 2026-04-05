using System;
using System.IO;

var hash = BCrypt.Net.BCrypt.HashPassword("admin123", 11);
Console.WriteLine("Generated hash: " + hash);
Console.WriteLine("Hash length: " + hash.Length);

// Write SQL file to fix admin password - using file to avoid PowerShell $ interpolation
var sql = $@"USE QuanLyDoanKham;
GO
SET NOCOUNT ON;
DELETE FROM Users WHERE Username = 'admin';
INSERT INTO Users (Username, PasswordHash, FullName, RoleId)
VALUES ('admin', '{hash}', N'Quản trị viên', 1);
GO
SELECT Username, LEN(PasswordHash) as HashLength FROM Users WHERE Username = 'admin';
GO
";
File.WriteAllText(@"d:\QuanLyDoanKham\fix_admin_final.sql", sql);
Console.WriteLine("SQL file written to d:\\QuanLyDoanKham\\fix_admin_final.sql");
