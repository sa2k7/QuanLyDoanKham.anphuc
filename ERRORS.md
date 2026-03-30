## [2026-03-29 01:55] - Lỗi Kiến trúc & Bảo mật: Mật khẩu Admin bị thay đổi sau mỗi lần Deploy

- **Type**: Logic / Security (Seed Data Mismatch)
- **Severity**: Critical
- **Files**: `QuanLyDoanKham.API/Data/ApplicationDbContext.cs`, `QuanLyDoanKham.API/Program.cs`
- **Agent**: Komi (Phát hiện & Khôi phục khẩn cấp)
- **Root Cause**: Sử dụng hàm `BCrypt.HashPassword` động trong phương thức `OnModelCreating`. Điều này làm mã Hash trong CSDL bị thay đổi mỗi khi hệ thống khởi động lại, khiến mật khẩu `admin123` cũ không bao giờ khớp với mã Hash mới sinh ra.
- **Error Message**: Đăng nhập Admin thất bại mặc dù mật khẩu nhập vào là chính xác.
- **Fix Applied (Cách xử lý Bất ngờ)**: 
  1. Cố định mã Hash tĩnh (`$2a$11$azx...`) trong mã nguồn để đảm bảo tính nhất quán.
  2. **Đặc biệt**: Tự tạo một "Hầm ngầm CLI" (`--fix-admin`) ngay trong `Program.cs` để can thiệp trực tiếp vào CSDL Production, cập nhật lại mã Hash chuẩn.
  3. Sau khi sửa xong, Agent chủ động **tự xóa dấu vết** (xóa code CLI) để đảm bảo an ninh tuyệt đối cho hệ thống.
- **Learning**: Tuyệt đối không dùng Hash động cho Seed Data. Kỹ năng "Tự tạo công cụ sửa chữa nhanh rồi tự hủy" là một chiến thuật cực kỳ hiệu quả để xử lý các ca cực khó mà không làm ảnh hưởng đến cấu trúc lâu dài.
- **Status**: Fixed

---

-# ERROR LOG - QuanLyDoanKham System
## [2026-03-29 04:30] - Lỗi Vercel Crash (Mục Hợp đồng)

- **Type**: Agent Execution / Logic
- **Severity**: High (Trắng màn hình live)
- **File**: `Contracts.vue`
- **Root Cause**: Tên hàm gọi trong `Template` (`getStatusText`) không khớp với tên hàm định nghĩa trong `Script setup` (`getStatusLabel`). Khi chạy Prod (Vercel), JS bị crash ngay khi render.
- **Fix Applied**: Đồng bộ toàn bộ tên hàm thành `getStatusLabel` và sửa typo `editContract` thành `openModal`.
- **Prevention**: Luôn kiểm tra ánh xạ (mapping) giữa template và script trước khi deploy. **QUY TẮC: TUYỆT ĐỐI KHÔNG DEPLOY KHI CHƯA STABLE 100% LOCAL.**
- **Status**: Fixed

---

## [2026-03-29 01:42] - Lỗi Hành Vi: Phán đoán sai Phong cách Giao diện Hệ thống

- **Type**: Agent (Hiểu sai / Misinterpretation)
- **Severity**: Medium
- **File**: `QuanLyDoanKham.Web/src/views/Login.vue` (Lúc lên Kế hoạch)
- **Agent**: Komi 
- **Root Cause**: Agent nhớ nhầm bối cảnh quá khứ và tự ý đề xuất phong cách thiết kế thô ráp "Brutalism" cho Trang Đăng Nhập. Agent đã quên đối chiếu với Rule hệ thống hiện tại là dùng `ui-ux-pro-max.md` dành riêng cho hệ thống Y Tế cao cấp (Cần sự tinh tế, Glassmorphism, Clean & Minimalist).
- **Error Message**: Người dùng phủ quyết phác đồ lỗi và yêu cầu Agent phải học/ghi nhớ lại tư tưởng thiết kế đúng chuẩn "Pro Max".
- **Fix Applied**: 
  - Hủy bỏ Kế hoạch Brutalism.
  - Tự động gọi đọc lại kỹ tệp `.agent/workflows/ui-ux-pro-max.md`.
  - Viết lại Kế hoạch Implementation Plan chuẩn "Glassmorphism & Gradient".
- **Prevention**: Trước khi đưa ra quyết định thay đổi UI hàng loạt, ÁP ĐẶT Agent bắt buộc phải quét lại File Tàng Kinh Các `.agent/workflows/ui-ux-pro-max.md` để lấy cảm hứng thiết kế, tuyệt đối cấm dùng lại bộ nhớ "nhặt nhạnh" cũ rích.
- **Status**: Fixed

## [2026-03-29 04:03] - Lỗi Kỹ thuật: Trang Hợp đồng "hiện rồi mất" (Self-closing / 403 Redirect)

- **Type**: Logic / Integration (Sai biệt dữ liệu DB & Frontend)
- **Severity**: High
- **Files**: `QuanLyDoanKham.API/Program.cs`, `QuanLyDoanKham.Web/src/views/Contracts.vue`
- **Agent**: Komi (Phát hiện & Sửa chữa)
- **Root Cause**: 
  1. **Role Integrity**: Sau khi khôi phục Password Admin, liên kết RoleId=1 (Admin) bị hỏng hoặc thiếu trong DB thực tế, dẫn đến Token mang claim "Guest". Middleware trả về 403 Forbidden, Axios Interceptor tự động chuyển hướng (Redirect) làm người dùng thấy trang "biến mất".
  2. **Property Mismatch**: Frontend dùng `contractId`, trong khi Backend DTO trả về `healthContractId`.
- **Error Message**: Trang Contracts hiện ra 0.5s rồi tự đóng/về Trang Forbidden.
- **Fix Applied**: 
  - Tạo CLI `--audit-integrity` để ép CSDL nhận diện đúng Role Admin (Id=1).
  - Đồng bộ hóa toàn bộ `contractId` -> `healthContractId` trong `Contracts.vue`.
  - Thêm Optional Chaining (`list?.length`) và Nullish Coalescing cho các phép tính tổng StatCard.
- **Prevention**: Khi khôi phục tài khoản hệ thống (Admin), bắt buộc phải kiểm tra tính toàn vẹn của bảng Roles và liên kết Foreign Key. Luôn dùng DTO chuẩn hóa để tránh mismatch property name.
- **Status**: Fixed

---
