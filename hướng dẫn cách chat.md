# GUIDE_INTERACTION.md - Giao Thức Tương Tác Tối Ưu (User & komi)

> **Mục tiêu**: Giúp bạn truyền đạt ý đồ chuẩn xác để AI xử lý code hiệu quả nhất, giảm thiểu lỗi và thời gian chỉnh sửa.

---

## 🏛️ 1. PHONG CÁCH THIẾT KẾ (UI/UX)

Dự án này áp dụng quy chuẩn **Komi-style (Premium Visuals)**. Khi yêu cầu sửa giao diện, hãy nhớ:

1.  **Nút hành động chính (Primary)**: Sử dụng Gradient (Teal/Indigo) hoặc `bg-primary`.
2.  **Nút hành động phụ (Secondary)**: Sử dụng outline hoặc `bg-slate-50`.
3.  **Nút Hủy/Xóa (Danger/Cancel)**: Luôn dùng tông **Rose (Hồng đỏ)** để dễ nhận biết:
    - Code mẫu: `class="bg-rose-50 border-2 border-rose-100 text-rose-600 hover:bg-rose-600 hover:text-white"`
4.  **Bo góc (Rounding)**: Sử dụng `rounded-xl` cho form và `rounded-2xl` cho modal/card lớn.

---

## 💬 2. CÔNG THỨC CHAT "THẦN THÁNH"

### A. Khi báo lỗi (Bug Report)
> "Tôi gặp lỗi ở [Trang/File]. Hiện tượng là [Mô tả]. Đầu vào là [Data]. Hãy kiểm tra [Gợi ý nguyên nhân nếu biết]."

### B. Khi sửa giao diện đồng loạt
> "Hãy sửa [Thành phần] theo style [Style] và **đồng bộ (Sync)** cho toàn bộ module này."

### C. Khi thêm tính năng mới
> "Dùng lệnh `/plan` để thiết lập quy trình tính năng [Tên]. Yêu cầu là [Mô tả logic]. Nhớ check ảnh hưởng đến [Module liên quan]."

---

## 🐚 3. CÁC TỪ KHÓA ĐIỀU HƯỚNG (COMMANDS)

- **`Đồng bộ / Toàn cục`**: AI sẽ quét toàn bộ dự án để sửa đồng nhất.
- **`Check-all`**: AI sẽ kiểm tra cả Backend + Frontend cho một thay đổi.
- **`Komi-style`**: AI sẽ tự động dùng các màu sắc Premium (Indigo, Teal, Rose, Slate).
- **`/plan`**: Bắt buộc AI lập kế hoạch trước khi động vào code.

---

## 🏥 4. LƯU Ý ĐẶC THÙ DỰ ÁN (QuanLyDoanKham)

1.  **RBAC (Phần quyền)**: Luôn dùng `authStore.userRole` hoặc `authStore.hasRole('Name')`. KHÔNG dùng `authStore.role`.
2.  **API Client**: Luôn dùng `@/services/apiClient`.
3.  **Thông báo**: Dùng `toast.success` hoặc `toast.error` từ `useToast`.

---

*Tài liệu này được tạo vào: 2026-04-07. Hãy nhắc komi xem lại file này nếu thấy nó làm lệch style.*
