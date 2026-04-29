---
trigger: vue_file_modification
---

# PREMIUM-DESIGN.MD - Quy Chuẩn Giao Diện Premium (komi style)

> **Mục tiêu**: Đảm bảo toàn bộ ứng dụng QuanLyDoanKham có trải nghiệm Visual Premium, WOW người dùng ngay từ cái nhìn đầu tiên.

---

## 🎨 1. QUY TẮC HIỂN THỊ DỮ LIỆU (SMART FILTERING)

Trong các màn hình giao dịch (như Lập Phiếu Kho, Điều động nhân sự...), tuyệt đối **KHÔNG** hiển thị toàn bộ danh sách hỗn loạn:

1. **Lọc Trạng thái**: Chỉ hiển thị các thực thể (Đoàn khám, Hợp đồng, Vật tư) đang ở trạng thái hoạt động (**Status === 'Open'**).
2. **Loại bỏ Rác**: Các đoàn đã hoàn tất (`Finished`), đã khóa (`Locked`) hoặc hợp đồng đã cũ không được đưa vào danh sách chọn để tránh nhầm lẫn cho người dùng.

---

## 🏗️ 2. CẤU TRÚC BỘ CHỌN (CUSTOM SELECT UI)

Cấm sử dụng thẻ `<select>` mặc định của trình duyệt. Thay vào đó, sử dụng cấu trúc **Custom Dropdown** với các đặc điểm:

1. **Badge Nhận diện**: Mã đoàn hoặc Mã vật tư phải được đặt trong một `<div>` có màu nền đậm (Indigo/Violet) và bo góc mềm mại.
2. **Phân cấp thông tin (Hierarchy)**:
   - **Dòng 1**: Tên chính (Chữ đậm, uppercase, font-black).
   - **Dòng 2**: Thông tin phụ như Tên công ty, Ngày tháng (Chữ nhỏ, màu slate-400, italic hoặc uppercase nhẹ).
3. **Hiệu ứng**:
   - Bo góc cực đại: `rounded-2xl` hoặc `rounded-3xl` cho container.
   - Hiệu ứng kính mờ: `backdrop-blur-xl` kết hợp với `bg-white/80`.
   - Đổ bóng sâu: `shadow-2xl` cho menu dropdown khi xổ xuống.

---

## 💎 3. STYLE TOKENS (Premium UI)

- **Bo góc**: Luôn ưu tiên `rounded-2xl` cho Input và `rounded-[2rem]` cho Card/Modal lớn.
- **Màu sắc**:
  - Chính: **Indigo-600** hoặc **Violet-600**.
  - Trạng thái: Emerald (Thành công), Rose (Nguy hiểm), Amber (Cảnh báo).
- **Font**: Sử dụng font chữ đậm (**font-black**) cho các tiêu đề và định danh quan trọng.
- **Animations**:
  - `animate-fade-in` cho các phần tử xuất hiện.
  - `animate-scale-up` cho Modals và Dropdowns.
  - `transition-all` cho mọi hiệu ứng hover/active.

---

## ⚡ 4. LƯU Ý CHO AGENT (KOMI)

- Mỗi khi sửa file `.vue`, hãy rà soát xem có thẻ `<select>` nào cần được "Premium hóa" hay không.
- Luôn kiểm tra hàm `fetch` để đảm bảo dữ liệu được lọc sạch trước khi render ra UI.

---
*Bản hướng dẫn này là chuẩn mực thiết kế cao nhất cho dự án.*
