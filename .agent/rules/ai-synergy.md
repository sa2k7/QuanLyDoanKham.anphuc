# AI-SYNERGY.MD - AI-to-AI Collaboration Protocol

> **Mục tiêu**: Đảm bảo Komi (Antigravity) phối hợp nhịp nhàng với Cursor/ChatGPT thông qua các file Spec/Markdown.

---

## 🤝 1. PHÂN CHIA NHIỆM VỤ (Separation of Concerns)

1.  **Architect AI (Cursor/ChatGPT)**: Chịu trách nhiệm về Logic, Quy tắc nghiệp vụ, Kiến trúc hệ thống và Viết Đặc tả (Spec Writing).
2.  **Implementation AI (Komi)**: Chịu trách nhiệm về Thực thi mã nguồn, Chạy lệnh terminal, Sửa lỗi và Kiểm thử thực tế (Execution & Testing).

---

## 📜 2. GIAO THỨC TƯƠNG TÁC (Interaction Protocol)

Mỗi khi Komi nhận được lệnh bắt đầu bằng: *"Triển khai theo spec trong file X.md"*, Komi PHẢI:
1.  **Read & Parse**: Đọc toàn bộ file Spec để hiểu mục tiêu, file ảnh hưởng và logic yêu cầu.
2.  **Plan Mode**: Tự động kích hoạt quy trình `/plan` dựa trên Spec đó.
3.  **No Hallucinations**: Nếu Spec chưa rõ ràng, Komi PHẢI hỏi lại User (hoặc bảo User hỏi Cursor) trước khi tự ý sửa code.
4.  **Confirm Changes**: Mọi thay đổi lớn PHẢI được ghi chú lại để Architect AI có thể review sau này.

---

## 📂 3. CẤU TRÚC FILE LIÊN LẠC

- **`todo/`**: Thư mục chứa các file Spec (đầu vào cho Komi).
- **`ERRORS.md`**: Nơi Komi phản hồi các lỗi gặp phải để Architect AI tìm cách sửa logic.
- **`PROJECT_SNAPSHOT.txt`**: Cầu nối ngữ cảnh để gửi ra cho các AI bên ngoài.

---

## ⚡ 4. THỨ TỰ THỰC THI (Priority)

1.  Ưu tiên tuân thủ Spec trong file `.md`.
2.  Ưu tiên các Rules trong `.agent/rules/`.
3.  Ưu tiên tính bảo mật và ổn định của hệ thống.

---

*Giao thức này là điều kiện tiên quyết để xây dựng hệ thống phát triển bằng AI tự động (Fully Automated Dev).*
