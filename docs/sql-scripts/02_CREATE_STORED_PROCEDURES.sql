-- =============================================
-- Script: Create Stored Procedures for NHANVIEN
-- Description: CRUD and Search operations
-- Author: System
-- Date: 2026-02-11
-- =============================================

USE QuanLyDoanKham;
GO

-- =============================================
-- Stored Procedure: Get All Employees
-- =============================================
IF OBJECT_ID('dbo.sp_NHANVIEN_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_NHANVIEN_GetAll;
GO

CREATE PROCEDURE dbo.sp_NHANVIEN_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        MaNV, IDBSYS, HoTen, HoTenKhongDau, NamSinh, GioiTinh, CMND,
        MaSoThue, SoTaiKhoan, TenTaiKhoan, NganHang, SoDienThoai,
        ChucDanh, DonVi, LoaiNhanVien,
        CMND_MatTruoc, CMND_MatSau, ChungChiHanhNghe,
        TrangThai, CreatedDate, ModifiedDate
    FROM dbo.NHANVIEN
    WHERE TrangThai = 1
    ORDER BY HoTen;
END
GO

-- =============================================
-- Stored Procedure: Get Employee By ID
-- =============================================
IF OBJECT_ID('dbo.sp_NHANVIEN_GetByID', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_NHANVIEN_GetByID;
GO

CREATE PROCEDURE dbo.sp_NHANVIEN_GetByID
    @MaNV NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        MaNV, IDBSYS, HoTen, HoTenKhongDau, NamSinh, GioiTinh, CMND,
        MaSoThue, SoTaiKhoan, TenTaiKhoan, NganHang, SoDienThoai,
        ChucDanh, DonVi, LoaiNhanVien,
        CMND_MatTruoc, CMND_MatSau, ChungChiHanhNghe,
        TrangThai, CreatedDate, ModifiedDate
    FROM dbo.NHANVIEN
    WHERE MaNV = @MaNV AND TrangThai = 1;
END
GO

-- =============================================
-- Stored Procedure: Insert Employee
-- =============================================
IF OBJECT_ID('dbo.sp_NHANVIEN_Insert', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_NHANVIEN_Insert;
GO

CREATE PROCEDURE dbo.sp_NHANVIEN_Insert
    @MaNV NVARCHAR(20),
    @IDBSYS NVARCHAR(50),
    @HoTen NVARCHAR(100),
    @HoTenKhongDau NVARCHAR(100),
    @NamSinh INT,
    @GioiTinh NVARCHAR(10),
    @CMND NVARCHAR(20),
    @MaSoThue NVARCHAR(20),
    @SoTaiKhoan NVARCHAR(50),
    @TenTaiKhoan NVARCHAR(100),
    @NganHang NVARCHAR(100),
    @SoDienThoai NVARCHAR(15),
    @ChucDanh NVARCHAR(100),
    @DonVi NVARCHAR(100),
    @LoaiNhanVien NVARCHAR(20),
    @CMND_MatTruoc NVARCHAR(500),
    @CMND_MatSau NVARCHAR(500),
    @ChungChiHanhNghe NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO dbo.NHANVIEN
        (
            MaNV, IDBSYS, HoTen, HoTenKhongDau, NamSinh, GioiTinh, CMND,
            MaSoThue, SoTaiKhoan, TenTaiKhoan, NganHang, SoDienThoai,
            ChucDanh, DonVi, LoaiNhanVien,
            CMND_MatTruoc, CMND_MatSau, ChungChiHanhNghe,
            TrangThai, CreatedDate
        )
        VALUES
        (
            @MaNV, @IDBSYS, @HoTen, @HoTenKhongDau, @NamSinh, @GioiTinh, @CMND,
            @MaSoThue, @SoTaiKhoan, @TenTaiKhoan, @NganHang, @SoDienThoai,
            @ChucDanh, @DonVi, @LoaiNhanVien,
            @CMND_MatTruoc, @CMND_MatSau, @ChungChiHanhNghe,
            1, GETDATE()
        );
        
        RETURN 1; -- Success
    END TRY
    BEGIN CATCH
        RETURN 0; -- Failure
    END CATCH
END
GO

-- =============================================
-- Stored Procedure: Update Employee
-- =============================================
IF OBJECT_ID('dbo.sp_NHANVIEN_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_NHANVIEN_Update;
GO

CREATE PROCEDURE dbo.sp_NHANVIEN_Update
    @MaNV NVARCHAR(20),
    @IDBSYS NVARCHAR(50),
    @HoTen NVARCHAR(100),
    @HoTenKhongDau NVARCHAR(100),
    @NamSinh INT,
    @GioiTinh NVARCHAR(10),
    @CMND NVARCHAR(20),
    @MaSoThue NVARCHAR(20),
    @SoTaiKhoan NVARCHAR(50),
    @TenTaiKhoan NVARCHAR(100),
    @NganHang NVARCHAR(100),
    @SoDienThoai NVARCHAR(15),
    @ChucDanh NVARCHAR(100),
    @DonVi NVARCHAR(100),
    @LoaiNhanVien NVARCHAR(20),
    @CMND_MatTruoc NVARCHAR(500),
    @CMND_MatSau NVARCHAR(500),
    @ChungChiHanhNghe NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE dbo.NHANVIEN
        SET 
            IDBSYS = @IDBSYS,
            HoTen = @HoTen,
            HoTenKhongDau = @HoTenKhongDau,
            NamSinh = @NamSinh,
            GioiTinh = @GioiTinh,
            CMND = @CMND,
            MaSoThue = @MaSoThue,
            SoTaiKhoan = @SoTaiKhoan,
            TenTaiKhoan = @TenTaiKhoan,
            NganHang = @NganHang,
            SoDienThoai = @SoDienThoai,
            ChucDanh = @ChucDanh,
            DonVi = @DonVi,
            LoaiNhanVien = @LoaiNhanVien,
            CMND_MatTruoc = @CMND_MatTruoc,
            CMND_MatSau = @CMND_MatSau,
            ChungChiHanhNghe = @ChungChiHanhNghe,
            ModifiedDate = GETDATE()
        WHERE MaNV = @MaNV AND TrangThai = 1;
        
        RETURN 1; -- Success
    END TRY
    BEGIN CATCH
        RETURN 0; -- Failure
    END CATCH
END
GO

-- =============================================
-- Stored Procedure: Delete Employee (Soft Delete)
-- =============================================
IF OBJECT_ID('dbo.sp_NHANVIEN_Delete', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_NHANVIEN_Delete;
GO

CREATE PROCEDURE dbo.sp_NHANVIEN_Delete
    @MaNV NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE dbo.NHANVIEN
        SET TrangThai = 0,
            ModifiedDate = GETDATE()
        WHERE MaNV = @MaNV;
        
        RETURN 1; -- Success
    END TRY
    BEGIN CATCH
        RETURN 0; -- Failure
    END CATCH
END
GO

-- =============================================
-- Stored Procedure: Search Employees
-- =============================================
IF OBJECT_ID('dbo.sp_NHANVIEN_Search', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_NHANVIEN_Search;
GO

CREATE PROCEDURE dbo.sp_NHANVIEN_Search
    @Keyword NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        MaNV, IDBSYS, HoTen, HoTenKhongDau, NamSinh, GioiTinh, CMND,
        MaSoThue, SoTaiKhoan, TenTaiKhoan, NganHang, SoDienThoai,
        ChucDanh, DonVi, LoaiNhanVien,
        CMND_MatTruoc, CMND_MatSau, ChungChiHanhNghe,
        TrangThai, CreatedDate, ModifiedDate
    FROM dbo.NHANVIEN
    WHERE TrangThai = 1
        AND (
            MaNV LIKE N'%' + @Keyword + '%'
            OR HoTen LIKE N'%' + @Keyword + '%'
            OR HoTenKhongDau LIKE N'%' + @Keyword + '%'
        )
    ORDER BY HoTen;
END
GO

-- =============================================
-- Stored Procedure: Check CMND Exists
-- =============================================
IF OBJECT_ID('dbo.sp_NHANVIEN_CheckCMNDExists', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_NHANVIEN_CheckCMNDExists;
GO

CREATE PROCEDURE dbo.sp_NHANVIEN_CheckCMNDExists
    @CMND NVARCHAR(20),
    @ExcludeMaNV NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Count INT;
    
    SELECT @Count = COUNT(*)
    FROM dbo.NHANVIEN
    WHERE CMND = @CMND 
        AND TrangThai = 1
        AND (@ExcludeMaNV IS NULL OR MaNV != @ExcludeMaNV);
    
    RETURN @Count;
END
GO

PRINT 'All stored procedures created successfully!';
GO
