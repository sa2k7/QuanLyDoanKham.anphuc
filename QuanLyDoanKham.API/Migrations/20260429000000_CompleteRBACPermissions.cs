using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <summary>
    /// Migration: CompleteRBACPermissions
    /// Fixes incomplete RBAC by adding missing permissions and RolePermissions for all roles.
    /// Uses IF NOT EXISTS throughout to be idempotent and safe.
    /// </summary>
    public partial class CompleteRBACPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ================================================================
            // STEP 1: Add missing permissions (idempotent)
            // ================================================================
            migrationBuilder.Sql("SET IDENTITY_INSERT [Permissions] ON");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=102 OR [PermissionKey]='HeThong.AuditLog') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(102,N'HeThong',N'HeThong.AuditLog',N'Xem nhật ký hệ thống')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=150 OR [PermissionKey]='Luong.View') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(150,N'Luong',N'Luong.View',N'Xem bảng lương')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=151 OR [PermissionKey]='Luong.Manage') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(151,N'Luong',N'Luong.Manage',N'Tính và duyệt lương')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=160 OR [PermissionKey]='NhanSu.View') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(160,N'NhanSu',N'NhanSu.View',N'Xem nhân sự')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=161 OR [PermissionKey]='NhanSu.Manage') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(161,N'NhanSu',N'NhanSu.Manage',N'Quản lý nhân sự')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=170 OR [PermissionKey]='KetQua.Write') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(170,N'KetQua',N'KetQua.Write',N'Ghi kết quả khám')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=171 OR [PermissionKey]='KetQua.QCApprove') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(171,N'KetQua',N'KetQua.QCApprove',N'QC phê duyệt kết quả')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionId]=180 OR [PermissionKey]='BenhNhan.View') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(180,N'BenhNhan',N'BenhNhan.View',N'Xem bệnh nhân')");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey]='DieuPhoi.Edit') INSERT INTO [Permissions]([PermissionId],[Module],[PermissionKey],[PermissionName]) VALUES(190,N'DieuPhoi',N'DieuPhoi.Edit',N'Chỉnh sửa điều phối')");
            migrationBuilder.Sql("SET IDENTITY_INSERT [Permissions] OFF");

            // ================================================================
            // STEP 2: Add RolePermissions using a single idempotent SQL block
            // All inserts check: Role exists, Permission exists, mapping not already present
            // ================================================================
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [RolePermissions] ON;

-- Helper macro: insert only if role, permission exist and mapping not duplicate
-- Admin (RoleId=1) - new permissions
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=1) BEGIN
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=102) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=102) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1038)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1038,1,102);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=150) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=150) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1039)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1039,1,150);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=151) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=151) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1040)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1040,1,151);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1041)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1041,1,160);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=161) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=161) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1042)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1042,1,161);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=170) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=170) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1043)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1043,1,170);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=171) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=171) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1044)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1044,1,171);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=180) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=180) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1045)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1045,1,180);
  -- DieuPhoi.Edit for Admin
  DECLARE @dpId INT = (SELECT TOP 1 [PermissionId] FROM [Permissions] WHERE [PermissionKey]='DieuPhoi.Edit');
  IF @dpId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=1 AND [PermissionId]=@dpId)
  BEGIN
    DECLARE @newId INT = ISNULL((SELECT MAX([Id]) FROM [RolePermissions]),0) + 1;
    IF @newId < 1046 SET @newId = 1046;
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(@newId,1,@dpId);
  END
END

-- ContractManager (RoleId=3)
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=3) BEGIN
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=3 AND [PermissionId]=10) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1100)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1100,3,10);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=3 AND [PermissionId]=40) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1101)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1101,3,40);
END

-- MedicalGroupManager (RoleId=5)
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=5) BEGIN
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=5 AND [PermissionId]=32) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1200)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1200,5,32);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=5 AND [PermissionId]=40) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1201)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1201,5,40);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=5 AND [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1202)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1202,5,160);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=180) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=5 AND [PermissionId]=180) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1203)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1203,5,180);
END

-- PersonnelManager (RoleId=2)
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=2) BEGIN
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=2 AND [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1300)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1300,2,160);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=161) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=2 AND [PermissionId]=161) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1301)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1301,2,161);
END

-- PayrollManager (RoleId=4)
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=4) BEGIN
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=4 AND [PermissionId]=32) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1400)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1400,4,32);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=4 AND [PermissionId]=40) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1401)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1401,4,40);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=150) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=4 AND [PermissionId]=150) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1402)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1402,4,150);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=151) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=4 AND [PermissionId]=151) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1403)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1403,4,151);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=4 AND [PermissionId]=160) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1404)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1404,4,160);
END

-- GroupLeader (RoleId=7)
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=7) BEGIN
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=7 AND [PermissionId]=15) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1700)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1700,7,15);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=7 AND [PermissionId]=20) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1701)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1701,7,20);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=7 AND [PermissionId]=30) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1702)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1702,7,30);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=7 AND [PermissionId]=31) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1703)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1703,7,31);
END

-- MedicalStaff (RoleId=8)
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=8) BEGIN
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=8 AND [PermissionId]=20) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1800)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1800,8,20);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=8 AND [PermissionId]=31) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1801)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1801,8,31);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=170) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=8 AND [PermissionId]=170) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1802)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1802,8,170);
END

-- Accountant (RoleId=9)
IF EXISTS(SELECT 1 FROM [Roles] WHERE [RoleId]=9) BEGIN
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=9 AND [PermissionId]=10) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1900)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1900,9,10);
  IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=9 AND [PermissionId]=50) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1901)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1901,9,50);
  IF EXISTS(SELECT 1 FROM [Permissions] WHERE [PermissionId]=150) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [RoleId]=9 AND [PermissionId]=150) AND NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [Id]=1902)
    INSERT INTO [RolePermissions]([Id],[RoleId],[PermissionId]) VALUES(1902,9,150);
END

SET IDENTITY_INSERT [RolePermissions] OFF;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            int[] rpIds = { 1038,1039,1040,1041,1042,1043,1044,1045,1046,
                            1100,1101, 1200,1201,1202,1203, 1300,1301,
                            1400,1401,1402,1403,1404, 1700,1701,1702,1703,
                            1800,1801,1802, 1900,1901,1902 };
            foreach (var id in rpIds)
                migrationBuilder.Sql($"DELETE FROM [RolePermissions] WHERE [Id]={id}");

            int[] permIds = { 102, 150, 151, 160, 161, 170, 171, 180, 190 };
            foreach (var id in permIds)
                migrationBuilder.Sql($"IF NOT EXISTS(SELECT 1 FROM [RolePermissions] WHERE [PermissionId]={id}) DELETE FROM [Permissions] WHERE [PermissionId]={id}");
        }
    }
}
