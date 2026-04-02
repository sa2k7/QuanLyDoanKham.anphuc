-- =============================================
-- Script: Create NHANVIEN Table
-- Description: Employee Management Table with all required fields
-- Author: System
-- Date: 2026-02-11
-- =============================================

USE QuanLyDoanKham;
GO

-- Drop table if exists (for development only)
IF OBJECT_ID('dbo.NHANVIEN', 'U') IS NOT NULL
    DROP TABLE dbo.NHANVIEN;
GO

-- Create NHANVIEN table
CREATE TABLE dbo.NHANVIEN
(
    -- Primary Key
    MaNV NVARCHAR(20) NOT NULL PRIMARY KEY,
    
    -- Personal Information
    IDBSYS NVARCHAR(50) NULL,
    HoTen NVARCHAR(100) NOT NULL,
    HoTenKhongDau NVARCHAR(100) NULL,
    NamSinh INT NULL,
    GioiTinh NVARCHAR(10) NULL,
    CMND NVARCHAR(20) NULL UNIQUE,
    
    -- Financial Information
    MaSoThue NVARCHAR(20) NULL,
    SoTaiKhoan NVARCHAR(50) NULL,
    TenTaiKhoan NVARCHAR(100) NULL,
    NganHang NVARCHAR(100) NULL,
    
    -- Contact Information
    SoDienThoai NVARCHAR(15) NULL,
    
    -- Work Information
    ChucDanh NVARCHAR(100) NULL,
    DonVi NVARCHAR(100) NULL,
    LoaiNhanVien NVARCHAR(20) NULL, -- NoiBo / ThueNgoai
    
    -- Image Paths
    CMND_MatTruoc NVARCHAR(500) NULL,
    CMND_MatSau NVARCHAR(500) NULL,
    ChungChiHanhNghe NVARCHAR(500) NULL,
    
    -- Audit Fields
    TrangThai BIT NOT NULL DEFAULT 1, -- 1: Active, 0: Deleted
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME NULL
);
GO

-- Create indexes for better search performance
CREATE NONCLUSTERED INDEX IX_NHANVIEN_HoTen 
    ON dbo.NHANVIEN(HoTen);
GO

CREATE NONCLUSTERED INDEX IX_NHANVIEN_HoTenKhongDau 
    ON dbo.NHANVIEN(HoTenKhongDau);
GO

CREATE NONCLUSTERED INDEX IX_NHANVIEN_CMND 
    ON dbo.NHANVIEN(CMND);
GO

CREATE NONCLUSTERED INDEX IX_NHANVIEN_TrangThai 
    ON dbo.NHANVIEN(TrangThai);
GO

PRINT 'NHANVIEN table created successfully!';
GO
