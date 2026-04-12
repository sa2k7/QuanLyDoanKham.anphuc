# PROJECT SPECIFICATION (SSOT)

> **Dự án**: Quản lý Đoàn Khám (QuanLyDoanKham)
> **Vai trò**: Nguồn sự thật duy nhất cho cấu trúc dữ liệu và API.

---

## 🗄️ 1. DATABASE SCHEMA (Core Entities)

### Module: Auth & Permissions
- **Users**: `UserId`, `Username`, `PasswordHash`, `FullName`, `RoleId`, `CompanyId`, `IsActive`
- **Roles**: `RoleId`, `RoleName`, `Description`
- **Permissions**: `PermissionId`, `PermissionKey`, `PermissionName`, `Module`

### Module: Hợp đồng (Contracts)
- **Companies**: `CompanyId`, `CompanyName`, `ShortName`, `Address`, `ContactPerson`
- **HealthContracts**: `HealthContractId`, `ContractCode`, `CompanyId`, `TotalAmount`, `ExpectedQuantity`, `UnitPrice`, `Status` (Draft, Pending, Approved, Active, Finished, Rejected)

### Module: Đoàn khám (Medical Groups)
- **MedicalGroups**: `GroupId`, `GroupName`, `ExamDate`, `HealthContractId`, `Status` (Draft, Open, InProgress, Finished, Locked), `GroupLeaderStaffId`
- **MedicalRecords**: `MedicalRecordId`, `GroupId`, `PatientId`, `FullQrToken`, `QueueNo`, `Status` (WAITING, PROCESSING, COMPLETED, CANCELLED)
- **Stations**: `StationCode`, `StationName`, `ServiceType` (ADMIN, CLINICAL, LAB, IMAGING)

### Module: Vật tư & Chi phí
- **SupplyItems**: `ItemId`, `ItemName`, `Unit`, `CurrentStock`, `UnitPrice`
- **StockMovements**: `MovementId`, `ItemId`, `MovementType` (IN, OUT), `Quantity`, `UnitPrice`, `TotalValue`, `MedicalGroupId`
- **GroupCosts**: `CostId`, `GroupId`, `StaffCost`, `SupplyCost`, `OtherCost`, `TotalCost`

---

## 🌐 2. API MAP (Key Endpoints)

### Attendance & Check-In
- `GET /api/Attendance/active-qr-today`: Lấy QR đoàn đang diễn ra hôm nay.
- `GET /api/Attendance/patient-qr`: Lấy QR cho bệnh nhân tự báo danh.
- `POST /api/Attendance/checkin`: Chấm công/Báo danh qua QR.

### Medical Groups
- `GET /api/MedicalGroups`: Danh sách đoàn khám.
- `GET /api/MedicalGroups/{id}/qr`: Lấy QR nhanh cho 1 đoàn.
- `POST /api/MedicalGroups`: Tạo đoàn mới (Mặc định `Status="Open"`).

### Supplies
- `GET /api/Supplies`: Danh sách vật tư kho.
- `POST /api/Supplies/import`: Nhập kho vật tư.
- `POST /api/Supplies/export`: Xuất kho vật tư (Trừ tồn kho).
- `GET /api/Supplies/movements`: Nhật ký biến động kho toàn cục.

### Reporting
- `GET /api/Reporting/dashboard-kpis`: Thống kê KPI.
- `GET /api/Reporting/financial`: Báo cáo tài chính (Revenue vs Cost).

---

## 🚦 3. STATE MACHINE (Medical Records)
1. **CHECKIN**: Bệnh nhân quét mã QR -> Tạo MedicalRecord (`Status = WAITING`).
2. **STATIONS**: Bệnh nhân lần lượt qua các phòng khám.
3. **COMPLETED**: Sau khi hoàn thành tất cả StationTask.
4. **QC**: Kiểm tra hồ sơ lần cuối trước khi trả kết quả.
