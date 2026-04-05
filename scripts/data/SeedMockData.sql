-- =====================================================================
-- SEED DATA CHO MODULE "LỊCH CÁ NHÂN" - SQL SERVER
-- =====================================================================
USE QuanLyDoanKham;
GO

-- 1. TẠO TRẠM KHÁM MẪU (nếu chưa có)
IF NOT EXISTS (SELECT 1 FROM Departments WHERE DepartmentCode = 'SIEU_AM')
    INSERT INTO Departments (DepartmentName, DepartmentCode, Description, TotalStaff, TotalUsers) 
    VALUES (N'Trạm Siêu Âm', 'SIEU_AM', N'Thực hiện siêu âm ổ bụng, tim mạch và các cơ quan nội tạng', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Departments WHERE DepartmentCode = 'XET_NGHIEM')
    INSERT INTO Departments (DepartmentName, DepartmentCode, Description, TotalStaff, TotalUsers) 
    VALUES (N'Trạm Xét Nghiệm', 'XET_NGHIEM', N'Lấy mẫu máu và xét nghiệm các chỉ số sinh hóa', 1, 0);

IF NOT EXISTS (SELECT 1 FROM Departments WHERE DepartmentCode = 'KHAM_NOI')
    INSERT INTO Departments (DepartmentName, DepartmentCode, Description, TotalStaff, TotalUsers) 
    VALUES (N'Khám Nội Tổng Hợp', 'KHAM_NOI', N'Khám lâm sàng tổng quát, đo huyết áp, kiểm tra sức khỏe cơ bản', 0, 0);

-- 2. TẠO HỒ SƠ NHÂN SỰ CHO ADMIN (nếu chưa có)
IF NOT EXISTS (SELECT 1 FROM Staffs WHERE EmployeeCode = 'admin')
BEGIN
    DECLARE @DeptId INT;
    SELECT TOP 1 @DeptId = DepartmentId FROM Departments WHERE DepartmentCode = 'SIEU_AM';
    IF @DeptId IS NOT NULL
        INSERT INTO Staffs (FullName, EmployeeCode, PhoneNumber, Email, DepartmentId, BaseSalary, HireDate, Status)
        VALUES (N'Bác sĩ Admin', 'admin', '0901234567', 'admin@anphuc.vn', @DeptId, 15000000, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0), 'Active');
END

-- 3. GẮN StaffId vào AppUsers cho tài khoản admin
DECLARE @AdminStaffId INT;
SELECT TOP 1 @AdminStaffId = StaffId FROM Staffs WHERE EmployeeCode = 'admin';

IF @AdminStaffId IS NOT NULL
BEGIN
    UPDATE AppUsers 
    SET StaffId = @AdminStaffId
    WHERE Username = 'admin' AND (StaffId IS NULL OR StaffId = 0);
END

-- 4. TẠO ĐOÀN KHÁM MẪU (tháng này)
IF NOT EXISTS (SELECT 1 FROM MedicalGroups WHERE GroupName = N'Đoàn khám Điện lực Sài Gòn')
    INSERT INTO MedicalGroups (GroupName, ExamDate, Address, Note, Status, IsActive, CreatedAt)
    VALUES (N'Đoàn khám Điện lực Sài Gòn', CAST(GETDATE() AS DATE), N'KCN Tân Bình, HCM', N'Khám sức khoẻ thường niên 2026', 'Approved', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM MedicalGroups WHERE GroupName = N'Đoàn khám Giáo viên Q.Bình Thạnh')
    INSERT INTO MedicalGroups (GroupName, ExamDate, Address, Note, Status, IsActive, CreatedAt)
    VALUES (N'Đoàn khám Giáo viên Q.Bình Thạnh', CAST(GETDATE()-3 AS DATE), N'THPT Gia Định, HCM', N'Khám sức khoẻ thẻ ngành', 'Finished', 1, DATEADD(day, -4, GETDATE()));

-- 5. PHÂN CÔNG ADMIN VÀO ĐOÀN (GroupStaffDetails)
DECLARE @GroupId1 INT, @GroupId2 INT;
SELECT TOP 1 @GroupId1 = GroupId FROM MedicalGroups WHERE GroupName = N'Đoàn khám Điện lực Sài Gòn';
SELECT TOP 1 @GroupId2 = GroupId FROM MedicalGroups WHERE GroupName = N'Đoàn khám Giáo viên Q.Bình Thạnh';

IF @GroupId1 IS NOT NULL AND @AdminStaffId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM GroupStaffDetails WHERE GroupId = @GroupId1 AND StaffId = @AdminStaffId)
    INSERT INTO GroupStaffDetails (GroupId, StaffId, WorkPosition, WorkStatus, ExamDate, ShiftType, CalculatedSalary)
    VALUES (@GroupId1, @AdminStaffId, N'Bác sĩ Siêu âm', 'Pending', CAST(GETDATE() AS DATE), 1.0, 300000);

IF @GroupId2 IS NOT NULL AND @AdminStaffId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM GroupStaffDetails WHERE GroupId = @GroupId2 AND StaffId = @AdminStaffId)
    INSERT INTO GroupStaffDetails (GroupId, StaffId, WorkPosition, WorkStatus, ExamDate, ShiftType, CalculatedSalary)
    VALUES (@GroupId2, @AdminStaffId, N'Bác sĩ Siêu âm', 'Joined', CAST(GETDATE()-3 AS DATE), 1.0, 300000);

-- 6. TẠO LỊCH CHẤM CÔNG (ScheduleCalendars)
-- Lịch hôm nay
IF @GroupId1 IS NOT NULL AND @AdminStaffId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ScheduleCalendars WHERE GroupId = @GroupId1 AND StaffId = @AdminStaffId AND ExamDate = CAST(GETDATE() AS DATE))
    INSERT INTO ScheduleCalendars (GroupId, StaffId, ExamDate, CheckInTime, CheckOutTime, IsConfirmed, Note)
    VALUES (@GroupId1, @AdminStaffId, CAST(GETDATE() AS DATE), DATEADD(hour, 8, CAST(CAST(GETDATE() AS DATE) AS DATETIME)), NULL, 0, N'Check-in qua QR lúc 08:00');

-- Lịch 3 ngày trước
IF @GroupId2 IS NOT NULL AND @AdminStaffId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ScheduleCalendars WHERE GroupId = @GroupId2 AND StaffId = @AdminStaffId AND ExamDate = CAST(GETDATE()-3 AS DATE))
    INSERT INTO ScheduleCalendars (GroupId, StaffId, ExamDate, CheckInTime, CheckOutTime, IsConfirmed, Note)
    VALUES (@GroupId2, @AdminStaffId, CAST(GETDATE()-3 AS DATE), DATEADD(hour, 8, CAST(CAST(GETDATE()-3 AS DATE) AS DATETIME)), DATEADD(hour, 17, CAST(CAST(GETDATE()-3 AS DATE) AS DATETIME)), 1, N'Đủ công - 08:00 đến 17:00');

GO
