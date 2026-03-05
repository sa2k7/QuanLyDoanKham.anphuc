---
description: Quy trình triển khai tính năng và đồng bộ hóa hệ thống (SOP)
---

Để đảm bảo mọi tính năng được bàn giao đều hoạt động ngay lập tức, Agent PHẢI tuân thủ các bước sau:

### 1. Phân tích & Cập nhật Cấu trúc (Database/Models)
- **Kiểm tra**: Xác định xem tính năng có thay đổi Schema (thêm bảng, thêm cột) không.
- **Hành động**: 
  - Thực hiện thay đổi trong file `Entities.cs` hoặc `Models`.
  - // turbo
  - Chạy lệnh: `dotnet ef migrations add [Name] --project [PathToAPI]`
  - // turbo
  - Chạy lệnh: `dotnet ef database update --project [PathToAPI]`

### 2. Triển khai Logic Đồng bộ (Full-stack Integration)
- Cập nhật **DTOs** để khớp với cấu trúc mới.
- Cập nhật **Controllers** để xử lý nghiệp vụ.
- Cập nhật **Frontend (Vue/React)** để hiển thị và tương tác.
- Kiểm tra tính nhất quán của ID và dữ liệu JSON giữa Backend và Frontend.

### 3. Xác thực kỹ thuật (Technical Verification)
- **Build**: Chạy `dotnet build` để đảm bảo hệ thống không lỗi cú pháp.
- **Lint**: Kiểm tra file Frontend (Vue/CSS) để tránh lỗi tag mở/đóng hoặc CSS không hợp lệ.
- // turbo
  Chạy lệnh: `dotnet build [PathToAPI]`

### 4. Kiểm tra Vận hành & Khởi động lại (Runtime)
- // turbo
  Dừng tiến trình cũ: `Get-Process dotnet | Stop-Process -Force` (nếu trên Windows).
- // turbo
  Khởi động lại Backend: `dotnet run --project [PathToAPI]`
- Kiểm tra Log console để đảm bảo không có lỗi lúc runtime.

---
**LƯU Ý:** Tuyệt đối không bàn giao cho USER khi chưa hoàn thành trọn vẹn 4 bước trên.
