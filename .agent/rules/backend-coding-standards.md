# BACKEND-CODING-STANDARDS.MD - Tiêu Chuẩn Code Backend (.NET)

> **Mục tiêu**: Định hình phong cách thiết kế, kiến trúc và các quy ước bắt buộc cho Backend của QuanLyDoanKham nhằm đảm bảo code "Sạch", dễ bảo trì và dễ mở rộng.

---

## 🚫 1. FORBIDDEN ACTIONS (Cấm/Hạn chế)

1. **KHÔNG BAO GIỜ DÙNG `required` CHO EF CORE ENTITIES**:
   - Việc dùng từ khóa `required` trong C# 11+ với Entity Framework navigation properties sẽ gây ra vô số lỗi biên dịch `CS9035` khi truy vấn hoặc chọn (Select) dữ liệu không đầy đủ.
   - **Giải pháp băt buộc**: Khởi tạo biến bằng `= null!`.
   - *Ví dụ sai*: `public required Staff Staff { get; set; }`
   - *Ví dụ đúng*: `public Staff Staff { get; set; } = null!;`

2. **KHÔNG NÉM NGOẠI LỆ (EXCEPTION) ĐỂ XỬ LÝ LOGIC**:
   - Hạn chế tối đa `throw new Exception("Lỗi...")`.
   - **Giải pháp**: Xây dựng hoặc sử dụng `ApiResult<T>` (Result Pattern) để trả về trạng thái từ Service lên Controller.

3. **KHÔNG VIẾT LOGIC TRUY XUẤT DB TRONG CONTROLLER**:
   - Controller chỉ đóng vai trò nhận Http Request, Validator và gọi đúng hàm trong Service.

---

## 🏗️ 2. KIẾN TRÚC & PATTERN (Architecture)

1. **Layered Services**:
   - Chia nhỏ Service thành các Domain riêng biệt (VD: `MedicalGroupAutoAssignmentService`, `FinancialReportService`), tuyệt đối không dồn code thành một `GodService`.

2. **FluentValidation (Lộ trình)**:
   - Thay vì dùng quá nhiều khối `if` trong Controller để kiểm tra Request, mọi DTO đầu vào cần có một class `Validator` xử lý logic kiểm tra độc lập.

3. **Dependency Injection**:
   - Đảm bảo mọi Repository, Services đều được tiêm thông qua Constructor của Controller.

---

## 🛡️ 3. TỰ HỌC (Self-Improvement)
*Rule này được sinh ra từ quá trình Refactor và sửa lỗi `CS9035` ngày 09/04/2026. Mọi Agent từ nay trở về sau PHẢI áp dụng khi code Backend cho dự án.*
