-- =============================================
-- Script: Thêm dữ liệu mẫu cho hệ thống
-- Mục đích: Tạo dữ liệu demo để test các tính năng
-- =============================================

USE QuanLyDoanKham;
GO

-- 1. Thêm Công ty mẫu (nếu chưa có)
IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyId = 1)
BEGIN
    SET IDENTITY_INSERT Companies ON;
    INSERT INTO Companies (CompanyId, CompanyName, Address, TaxCode, PhoneNumber)
    VALUES 
        (1, N'Tập đoàn VinGroup', N'Hà Nội', '0101234567', '0243123456'),
        (2, N'FPT Software', N'TP. Hồ Chí Minh', '0307654321', '0283987654'),
        (3, N'Viettel Group', N'Hà Nội', '0100109106', '0243456789');
    SET IDENTITY_INSERT Companies OFF;
END
GO

-- 2. Thêm Hợp đồng mẫu
IF NOT EXISTS (SELECT 1 FROM Contracts WHERE HealthContractId = 1)
BEGIN
    SET IDENTITY_INSERT Contracts ON;
    INSERT INTO Contracts (HealthContractId, CompanyId, TotalAmount, PatientCount, StartDate, EndDate, IsFinished)
    VALUES 
        (1, 1, 500000000, 1000, DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, 335, GETDATE()), 0),
        (2, 2, 300000000, 500, DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, 350, GETDATE()), 0),
        (3, 3, 800000000, 2000, DATEADD(DAY, -60, GETDATE()), DATEADD(DAY, 305, GETDATE()), 0);
    SET IDENTITY_INSERT Contracts OFF;
END
GO

-- 3. Thêm Nhân viên Y tế mẫu
IF NOT EXISTS (SELECT 1 FROM Staffs WHERE StaffId = 1)
BEGIN
    SET IDENTITY_INSERT Staffs ON;
    INSERT INTO Staffs (StaffId, EmployeeCode, FullName, FullNameUnsigned, BirthYear, Gender, IDCardNumber, TaxCode, 
                        BankAccountNumber, BankAccountName, BankName, PhoneNumber, JobTitle, Department, 
                        EmployeeType, BaseSalary, IsActive)
    VALUES 
        (1, 'BS001', N'Nguyễn Văn An', 'NGUYEN VAN AN', 1985, N'Nam', '001085012345', '0123456789', 
         '1234567890', N'NGUYEN VAN AN', N'Vietcombank', '0912345678', N'Bác sĩ', N'Nội khoa', N'Nội bộ', 500000, 1),
        (2, 'DD001', N'Trần Thị Bình', 'TRAN THI BINH', 1990, N'Nữ', '001090023456', '0234567890', 
         '2345678901', N'TRAN THI BINH', N'Techcombank', '0923456789', N'Điều dưỡng', N'Khám tổng quát', N'Nội bộ', 300000, 1),
        (3, 'KTV001', N'Lê Văn Cường', 'LE VAN CUONG', 1992, N'Nam', '001092034567', '0345678901', 
         '3456789012', N'LE VAN CUONG', N'ACB', '0934567890', N'Kỹ thuật viên', N'Xét nghiệm', N'Thuê ngoài', 250000, 1);
    SET IDENTITY_INSERT Staffs OFF;
END
GO

-- 4. Thêm Vật tư mẫu
IF NOT EXISTS (SELECT 1 FROM Supplies WHERE SupplyId = 1)
BEGIN
    SET IDENTITY_INSERT Supplies ON;
    INSERT INTO Supplies (SupplyId, SupplyName, IsFixedAsset, UnitPrice, StockQuantity)
    VALUES 
        (1, N'Găng tay y tế', 0, 5000, 10000),
        (2, N'Khẩu trang y tế', 0, 3000, 15000),
        (3, N'Máy đo huyết áp', 1, 2000000, 50),
        (4, N'Que thử đường huyết', 0, 50000, 5000),
        (5, N'Ống nghiệm', 0, 2000, 20000);
    SET IDENTITY_INSERT Supplies OFF;
END
GO

-- 5. Thêm Đoàn khám mẫu
IF NOT EXISTS (SELECT 1 FROM MedicalGroups WHERE GroupId = 1)
BEGIN
    SET IDENTITY_INSERT MedicalGroups ON;
    INSERT INTO MedicalGroups (GroupId, HealthContractId, GroupName, ExamDate, TotalCost, IsCompleted)
    VALUES 
        (1, 1, N'Đoàn khám VinGroup - Đợt 1', DATEADD(DAY, -10, GETDATE()), 0, 0),
        (2, 2, N'Đoàn khám FPT - Đợt 1', DATEADD(DAY, -5, GETDATE()), 0, 0);
    SET IDENTITY_INSERT MedicalGroups OFF;
END
GO

-- 6. Thêm Bệnh nhân mẫu
IF NOT EXISTS (SELECT 1 FROM Patients WHERE PatientId = 1)
BEGIN
    SET IDENTITY_INSERT Patients ON;
    INSERT INTO Patients (PatientId, HealthContractId, FullName, DateOfBirth, Gender, IDCardNumber, PhoneNumber, Department, CreatedDate)
    VALUES 
        (1, 1, N'Phạm Văn Đức', '1995-05-15', N'Nam', '001095045678', '0945678901', N'Phòng Kế toán', GETDATE()),
        (2, 1, N'Hoàng Thị Lan', '1998-08-20', N'Nữ', '001098056789', '0956789012', N'Phòng Nhân sự', GETDATE()),
        (3, 2, N'Vũ Minh Tuấn', '1993-03-10', N'Nam', '001093067890', '0967890123', N'Phòng IT', GETDATE());
    SET IDENTITY_INSERT Patients OFF;
END
GO

-- 7. Thêm User mẫu (Customer role)
IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = 2)
BEGIN
    SET IDENTITY_INSERT Users ON;
    INSERT INTO Users (UserId, Username, PasswordHash, FullName, RoleId, CompanyId)
    VALUES 
        (2, 'vingroup', '$2a$11$YourHashedPasswordHere', N'Quản lý VinGroup', 3, 1),
        (3, 'fpt', '$2a$11$YourHashedPasswordHere', N'Quản lý FPT', 3, 2);
    SET IDENTITY_INSERT Users OFF;
END
GO

PRINT N'✅ Đã thêm dữ liệu mẫu thành công!';
PRINT N'';
PRINT N'📋 THÔNG TIN ĐĂNG NHẬP:';
PRINT N'━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT N'👤 Admin (Quản trị viên):';
PRINT N'   Username: admin';
PRINT N'   Password: admin123';
PRINT N'   Role: Admin (Toàn quyền)';
PRINT N'';
PRINT N'👤 VinGroup (Khách hàng):';
PRINT N'   Username: vingroup';
PRINT N'   Password: vingroup123';
PRINT N'   Role: Customer (Chỉ xem hợp đồng của mình)';
PRINT N'';
PRINT N'👤 FPT (Khách hàng):';
PRINT N'   Username: fpt';
PRINT N'   Password: fpt123';
PRINT N'   Role: Customer (Chỉ xem hợp đồng của mình)';
PRINT N'━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
GO
