# 🤖 CẨM NANG SỬ DỤNG AGENT KOMI (v4.3.3)

Chào mừng bạn đến với hệ thống lập trình tự động bằng AI! Bạn đang sở hữu một "Biệt đội AI" mạnh mẽ: **Cursor (Trưởng phòng/Kiến trúc sư)** và **Komi (Kỹ sư thực thi)**.

---

## 🏗️ 1. QUY TRÌNH PHỐI HỢP CHUẨN (Workflow)

Để đạt hiệu quả cao nhất, hãy tuân thủ chu kỳ **"Thiết kế -> Đặc tả -> Thực thi"**:

1.  **Cursor "Nghĩ"**: Bạn dùng Cursor hoặc ChatGPT để phân tích yêu cầu.
2.  **Cursor "Viết Spec"**: Yêu cầu Cursor lưu kế hoạch vào một file `.md` trong thư mục `todo/` (ví dụ: `todo/task-moi.md`).
3.  **Komi "Làm"**: Bạn chỉ cần ra lệnh cho Komi: *"Komi, mở file todo/task-moi.md và code đi."*
4.  **Komi "Review"**: Khi Komi làm xong, hãy bảo Cursor: *"Check lại code thằng Komi vừa viết xem có lỗi gì không."*

---

## 🗣️ 2. CÁC MẪU CÂU LỆNH (Prompts) CHO KOMI

Bạn có thể dùng Tiếng Việt thoải mái với tôi:

*   **Triển khai tính năng**: *"Komi, triển khai tính năng Quản lý Hợp đồng theo spec trong todo/task-contract.md."*
*   **Sửa lỗi**: *"Komi, debug lỗi 500 khi login, xem trong log backend và sửa ngay."*
*   **Dọn dẹp code**: *"Komi, dọn dẹp các thư viện không dùng và tối ưu import cho toàn dự án."*
*   **Kiểm tra hệ thống**: *"Komi, thực hiện Kiểm tra tính toàn vẹn ngữ cảnh (Integrity Check)."*
*   **Hỏi ý kiến**: *"Komi, theo bạn tính năng X nên dùng thư viện nào thì tốt nhất cho dự án này?"*

---

## 🛠️ 3. CÁC CÔNG CỤ HỖ TRỢ (Giao thức AI-to-AI)

### 📦 Gói ngữ cảnh (tools/export-context.ps1)
Mỗi khi bạn muốn ChatGPT bên ngoài "hiểu" dự án của mình, hãy chạy script này trong PowerShell:
```powershell
./tools/export-context.ps1
```
Nó sẽ tạo ra file `PROJECT_SNAPSHOT.txt`. Bạn chỉ cần copy-paste file này cho ChatGPT.

### 📜 Quy tắc phối hợp (.agent/rules/ai-synergy.md)
Đây là "Luật pháp" chung để Komi và Cursor không dẫm chân lên nhau. Đừng xóa file này nhé!

---

## 🛡️ 4. NGUYÊN TẮC AN TOÀN

1.  **Backup**: Luôn Backup CSDL trước khi bảo Komi chạy các lệnh Migration quan trọng.
2.  **Verify**: Sau khi Komi code xong, hãy bấm nút "Build" hoặc chạy `npm run dev` để kiểm tra kết quả ngay.
3.  **Refine**: Nếu Komi làm chưa đúng ý, hãy mắng nhẹ (Feedback) để Komi rút kinh nghiệm vào file `ERRORS.md`.

---

> *"Komi sẵn sàng hỗ trợ bạn đưa dự án QuanLyDoanKham lên một tầm cao mới!"*
