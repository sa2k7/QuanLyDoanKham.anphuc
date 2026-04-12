# Nhật Ký Lỗi (ERRORS.md) - Dự Án Quản Lý Đoàn Khám

## [2026-04-10 13:50] - Lỗi Foreign Key: Truyền TaskId = 0 khi tạo Event

- **Type**: Logic / Database Integrity
- **Severity**: Critical (Gây crash API luồng Check-in/Finalize)
- **File**: `QuanLyDoanKham.Api/Services/MedicalRecords/MedicalRecordStateMachine.cs`
- **Agent**: Komi (Phát hiện & Sửa chữa)
- **Root Cause**: Khi không tìm thấy Task phù hợp, hệ thống sử dụng toán tử `?.TaskId ?? 0` để gán vào Event. Giá trị `0` vi phạm ràng buộc khóa ngoại (Foreign Key) trên SQL Server, dẫn đến DBUpdateException.
- **Error Message**: `The INSERT statement conflicted with the FOREIGN KEY constraint "FK_StationTaskEvents_RecordStationTasks_TaskId".`
- **Fix Applied**: 
  1. Áp dụng cơ chế **Fail-fast**: Kiểm tra Task tồn tại trước khi tạo Event.
  2. Dùng `ServiceResult.Failure()` để trả về lỗi nghiệp vụ (400) thay vì ném Exception (500).
- **Prevention**: Tuyệt đối không dùng giá trị mặc định (như 0) cho các trường Khóa ngoại. Phải validate sự tồn tại trước khi thao tác DB.
- **Status**: Fixed

---

## [2026-04-10 14:15] - Lỗi Logic EF Core: Stale Read do trễ nhịp SaveChanges

- **Type**: Logic / Entity Framework Sync
- **Severity**: High (Làm sai lệch trạng thái hồ sơ)
- **File**: `QuanLyDoanKham.Api/Services/MedicalRecords/MedicalRecordStateMachine.cs`
- **Agent**: Komi (Architect Review phát hiện)
- **Root Cause**: Thay đổi trạng thái Task trên RAM nhưng gọi `AnyAsync()` (truy vấn trực tiếp DB) trước khi gọi `SaveChangesAsync()`. Kết quả trả về từ DB bị cũ (Stale), dẫn đến biến `allDone` luôn sai.
- **Error Message**: Hồ sơ không bao giờ chuyển sang `WAITING_FOR_QC` dù đã hoàn tất mọi trạm.
- **Fix Applied**: **Option B (Double Save)**. Gọi `SaveChangesAsync()` ngay sau khi cập nhật Task để commit xuống DB, sau đó mới gọi `AnyAsync()` để kiểm đếm tổng thể.
- **Prevention**: Luôn lưu trạng thái xuống DB trước khi thực hiện các lệnh Query (Any, Count, Sum) phụ thuộc vào trạng thái đó trong cùng một Transaction.
- **Status**: Fixed

---

## [2026-04-01 10:20] - Lỗi Phản hồi API: Mismatch Naming Policy (PascalCase vs camelCase)

- **Type**: Integration / Configuration
- **Severity**: Medium (Khiến FE không lưu được Token)
- **Files**: `QuanLyDoanKham.API/Program.cs`, `QuanLyDoanKham.Web/src/stores/auth.js`
- **Agent**: Komi (Phát hiện & Sửa chữa)
- **Root Cause**: Backend mặc định trả về PascalCase (Token, Username) trong khi Frontend (Axios) mong đợi camelCase (token, username). 
- **Error Message**: Đăng nhập báo thành công ở Network nhưng UI vẫn báo lỗi hoặc không chuyển trang.
- **Fix Applied**: Cấu hình `JsonNamingPolicy.CamelCase` trong `AddJsonOptions` tại `Program.cs`.
- **Prevention**: Luôn kiểm tra PropertyNamingPolicy khi bắt đầu dự án Fullstack. Ưu tiên áp chuẩn camelCase cho toàn bộ API để đồng bộ với Javascript.
- **Status**: Fixed

---

## [2026-04-01 10:28] - Lỗi Thực thi: Truncation do Shell Interpolation (Ký tự $)

- **Type**: Agent Execution (Thao tác Terminal sai)
- **Severity**: Medium (Làm hỏng dữ liệu PasswordHash)
- **File**: Database (sqlcmd command line)
- **Agent**: Komi (Rút kinh nghiệm & Sửa chữa)
- **Root Cause**: Sử dụng sqlcmd với nháy kép (" ") trong PowerShell để Update mã băm BCrypt. Các ký tự $ trong mã băm bị Shell hiểu nhầm là biến môi trường và bị xóa trống, dẫn đến chuỗi bị cắt cụt (60 ký tự còn 34 ký tự).
- **Error Message**: 401 Unauthorized trên Swagger dù mã băm trông có vẻ đúng (nhưng thực tế bị thiếu đầu).
- **Fix Applied**: Chuyển sang dùng tệp .sql trung gian để thực hiện lệnh Update, tránh hoàn toàn sự can thiệp của Shell.
- **Prevention**: **QUY TẮC VÀNG**: Tuyệt đối không truyền chuỗi có ký tự đặc biệt ($, &, |) trực tiếp qua tham số dòng lệnh. Luôn dùng tệp Script hoặc Escaping chuẩn POSIX/PowerShell.
- **Status**: Fixed

---

## [2026-03-29 01:55] - Lỗi Kiến trúc & Bảo mật: Mật khẩu Admin bị thay đổi sau mỗi lần Deploy

- **Type**: Logic / Security (Seed Data Mismatch)
- **Severity**: Critical
- **Files**: `QuanLyDoanKham.API/Data/ApplicationDbContext.cs`, `QuanLyDoanKham.API/Program.cs`
- **Agent**: Komi (Phát hiện & Khôi phục khẩn cấp)
- **Root Cause**: Sử dụng hàm `BCrypt.HashPassword` động trong phương thức `OnModelCreating`. Điều này làm mã Hash trong CSDL bị thay đổi mỗi khi hệ thống khởi động lại.
- **Fix Applied**: Cố định mã Hash tĩnh trong mã nguồn. Tự tạo một "Hầm ngầm CLI" (`--fix-admin`) để can thiệp trực tiếp vào CSDL Production.
- **Status**: Fixed
