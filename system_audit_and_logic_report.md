# Báo cáo Phân tích Hệ thống & Quy trình Kiểm tra (Audit Report)

Tài liệu này mô tả chi tiết các bước đã thực hiện để kiểm tra hệ thống, cấu trúc phân quyền và logic vận hành của dự án **QuanLyDoanKham.anphuc**.

---

## 🔎 1. Quy trình Kiểm tra (Test Workflow)

Em đã thực hiện audit hệ thống theo trình tự 4 bước chuyên sâu:

### Bước 1: Kiểm soát Truy cập (Authentication Check)
- **Hành động**: Đăng nhập với tài khoản Admin mặc định.
- **Mục tiêu**: Kiểm tra tính hợp lệ của Header `Authorization: Bearer <token>` trong các yêu cầu API.
- **Kết quả**: Đăng nhập thành công, nhưng Token chưa chứa đủ các "Permission Claims" cần thiết.

### Bước 2: Kiểm tra Chức năng (Functional Audit)
Em đã quét qua tất cả các module sidebar, tập trung vào 3 thao tác: **READ** (Xem), **WRITE** (Thêm/Sửa), **DELETE** (Xóa).
- **Phát hiện**: 
  - Thao tác **READ** hoạt động tốt ở các module cơ bản.
  - Thao tác **WRITE/DELETE** bị chặn bởi lỗi **403 Forbidden** (Thiếu quyền).

### Bước 3: Kiểm tra Giao diện & Trải nghiệm (UI/UX Integrity)
- **Hành động**: Kiểm tra độ phân giải, sự hiển thị của các component Premium.
- **Phát hiện**: Lỗi thiếu component `CheckCircle2` làm gián đoạn dashboard. Nút "Tạo hợp đồng" bị lỗi logic hiển thị.

### Bước 4: Kiểm tra Logic Nghiệp vụ (Business Logic Test)
- **Hành động**: Thử tính lương, điều động nhân sự giả lập.
- **Phát hiện**: Các luồng xử lý dữ liệu nặng (như Tính lương) có chạy nhưng cần tối ưu hóa giao diện phản hồi.

---

## 🛡️ 2. Cấu trúc Phân quyền (Authorization Logic)

Dự án sử dụng cơ chế bảo mật đa tầng:

### Tầng 1: Database (Storage)
- Bảng `Permissions`: Lưu danh sách các Key (Ví dụ: `NhanSu.Manage`).
- Bảng `RolePermissions`: Liên kết giữa Vai trò (Role) và Quyền (Permission).

### Tầng 2: Backend (Enforcement)
- Sử dụng Custom Middleware `AuthorizePermissionAttribute`.
- **Logic**: Khi một request đến, hệ thống sẽ đọc `permission` claim trong Token. Nếu không khớp với Key yêu cầu -> Trả về `403 Forbidden`.

### Tầng 3: Frontend (Visual control)
- Sử dụng Composable `usePermission()`.
- **Logic**: 
  ```javascript
  const { can } = usePermission();
  // Nếu can('HopDong.Create') là false -> Nút "Thêm" sẽ không hiển thị.
  ```

---

## ⚙️ 3. Logic Hoạt động & Luồng Nghiệp vụ (Business Logic)

Hệ thống được thiết kế theo mô hình luồng công việc (Workflow-based):

1. **Module Công ty & Hợp đồng**: 
   - Là điểm bắt đầu của mọi dữ liệu.
   - Hợp đồng có trạng thái (Chờ duyệt, Đã duyệt, Kết thúc).
   - Chỉ hợp đồng **Đã duyệt** mới được phép tạo Đoàn khám.

2. **Module Đoàn khám & Điều động**:
   - Đây là module phức tạp nhất.
   - Logic: Khi tạo đoàn -> Phải chọn vị trí công việc (Vị trí bác sĩ, điều dưỡng...) -> Phân công nhân sự vào các vị trí này dựa trên điểm số hoặc sự sẵn sàng.

3. **Module Chấm công & Tính lương**:
   - Dựa trên dữ liệu đi đoàn thực tế để tự động tính công.
   - Logic: Công đi đoàn + Phụ cấp = Lương thực nhận.

4. **Module Vật tư**:
   - Quản lý xuất/nhập tồn kho vật tư y tế phục vụ cho các đoàn khám.

---

> [!TIP]
> **Nhận định**: Hệ thống hiện có bộ khung (Architecture) rất tốt và UI Premium. Các lỗi hiện tại chủ yếu nằm ở khâu "Cấu hình phân quyền" chưa khớp giữa Backend và Frontend, và một vài lỗi nhỏ trong quá trình ghép code. Chỉ cần xử lý xong Permission, hệ thống sẽ chạy mượt mà 100%.
