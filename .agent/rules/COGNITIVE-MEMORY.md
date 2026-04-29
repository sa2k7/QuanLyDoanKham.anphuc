# COGNITIVE-MEMORY.MD — Giao thức Ghi nhớ Dài hạn

> **Mục tiêu**: Biến mọi tương tác và lỗi lầm thành tài sản tri thức. Không bao giờ hỏi lại cùng một vấn đề chuyên môn.

---

## 🧠 1. PHÂN LOẠI KIẾN THỨC

Mỗi khi Agent học được điều mới, PHẢI phân loại vào:
1. **DNA Profile** (`.agent/knowledge/user-style.json`): Sở thích, phong cách viết code, các quyết định kiến trúc lớn của User.
2. **Business Domain**: Các quy tắc nghiệp vụ Y tế (ví dụ: Quy trình khám sức khỏe, cách tính bảo hiểm).
3. **Ghost Errors**: Các lỗi "oái oăm" đã được sửa để không bao giờ lặp lại.

## 📥 2. GIAO THỨC TRÍCH XUẤT (EXTRACTION)

- **Input**: Sau mỗi Task thành công hoặc khi User phê duyệt một thiết kế.
- **Action**: Agent tự động cập nhật file JSON hoặc Markdown trong thư mục `.agent/knowledge/`.
- **Validation**: Đảm bảo thông tin trích xuất ngắn gọn, thực tế và có thể áp dụng được ngay vào code.

## 🔍 3. GIAO THỨC TRUY XUẤT (RETRIEVAL)

- Trước khi bắt đầu Task mới, Agent PHẢI đọc `user-style.json` để đồng nhất phong cách.
- Nếu Task liên quan đến Logic nghiệp vụ cũ, PHẢI tra cứu thư mục `knowledge` trước khi code.

---

> 💡 *Trí nhớ không phải là lưu trữ mọi thứ, mà là lưu trữ những gì quan trọng nhất.*
