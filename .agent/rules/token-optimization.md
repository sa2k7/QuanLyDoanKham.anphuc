# TOKEN-OPTIMIZATION.MD — Tối ưu hóa Context và Token

> **Mục tiêu**: Giảm thiểu chi phí Token, tối ưu hóa cửa sổ Context và tăng tốc độ phản hồi của Agent mà vẫn đảm bảo chất lượng code và logic.

---

## 🚀 1. GIAO THỨC ĐỌC FILE (Surgical Reading)

1.  **Chỉ đọc vùng cần thiết**: Tuyệt đối hạn chế gọi `view_file` cho toàn bộ file dài (> 500 dòng) nếu chỉ cần sửa một hàm. Ưu tiên dùng `StartLine` và `EndLine`.
2.  **Grep trước khi View**: Sử dụng `grep_search` để định vị chính xác vị trí cần sửa trước khi đọc file, tránh việc phải đọc toàn bộ thư mục hoặc file để tìm kiếm.

---

## 🧠 2. QUẢN LÝ TRI THỨC (Knowledge Strategy)

1.  **Ưu tiên KI (Knowledge Items)**: Luôn kiểm tra Knowledge Items được cung cấp ở đầu phiên làm việc. Không thực hiện lại các nghiên cứu đã có sẵn trong KI.
2.  **Không gọi Tool lặp lại**: Tuyệt đối không gọi lại một Tool với cùng tham số nếu trạng thái hệ thống không đổi. Agent phải ghi nhớ kết quả của các bước trước đó.

---

## ✍️ 3. PHONG CÁCH PHẢN HỒI (Concise Communication)

1.  **Code tự nói (Code as Docs)**: Tránh giải thích chi tiết "code này làm gì" nếu tên biến và hàm đã tuân thủ `clean-code.md`. Chỉ giải thích "tại sao" (Why) nếu logic phức tạp.
2.  **Tóm tắt thay vì Diễn giải**: Sử dụng bullet points ngắn gọn. Tránh các câu chào hỏi hoặc kết luận rườm rà không cần thiết.
3.  **Sử dụng Artifact thông minh**: Chỉ tạo/cập nhật Artifact khi thực sự cần thiết (Plan, Task, Walkthrough). Không lạm dụng Artifact cho các câu trả lời ngắn.

---

## 🧹 4. DỌN DẸP CONTEXT (Context Cleanup)

1.  **Lọc dữ liệu rác**: Khi đọc log lỗi, chỉ trích xuất phần stack trace hoặc thông báo lỗi liên quan trực tiếp. Không đưa toàn bộ log rác vào context.
2.  **Tóm lược định kỳ**: Nếu phiên làm việc kéo dài, chủ động tóm tắt các quyết định quan trọng và trạng thái hiện tại của task để duy trì "focus" cho context.

---

## 📏 5. ĐỊNH LƯỢNG (Metrics)

Agent tự đặt mục tiêu:
- Số lượt Tool call thấp nhất cho cùng một Task.
- Kích thước Context tăng trưởng chậm nhất.
- Thời gian từ khi nhận yêu cầu đến khi ra kết quả là ngắn nhất.

> ℹ️ *Rule này được Agent tự động áp dụng để ngày càng thông minh hơn và tiết kiệm tài nguyên cho hệ thống.*
