# HEAVY-LAYERED-SYSTEM.MD — Kỷ luật Quản lý Hệ thống Đa tầng

> **Mục tiêu**: Áp dụng triết lý "Quản lý Agent thông qua Planning" để phát triển hệ thống tầm trung mà không bị rối.
> **Vibe**: Solo-Ninja / Scale-Mastery.

---

## 🦾 1. CỐT LÕI VẬN HÀNH (Orchestration First)

Thay vì cố gắng làm AI thông minh hơn, hãy tập trung vào việc quản lý chúng:

1.  **Nghiệp vụ là ưu tiên số 1**: Trước khi code bất kỳ layer nào (Service, Controller, Repos), Agent PHẢI trích xuất và hiểu rõ Business Logic/Features. Kiến thức chuyên môn là la bàn.
2.  **Plan-Only Worktree**: Mọi hành động bắt đầu bằng một bản Plan chi tiết. Khuyến khích thực hiện Planning trong một không gian tách biệt để tránh làm bẩn codebase chính khi chưa chốt phương án.
3.  **Visualization Mandate**: Mọi Task lớn PHẢI được mô tả bằng Mermaid Diagram trong `task.md`. Nếu không thấy được hình ảnh, đừng bắt đầu code.

---

## 🏗️ 2. CHIẾN THUẬT SANDBOX (Sandbox Strategy)

Để duy trì tốc độ phát triển mà không sợ "đập đi xây lại":

1.  **Worktree Sandboxing**: Chia nhỏ tính năng thành 10-20 "hộp cát" (Git Worktree). Mỗi hộp cát giải quyết một nghiệp vụ cụ thể.
2.  **Parallel Checking**: Thay vì chờ đợi CI/CD, hãy chạy song song nhiều bài kiểm tra kiểm tra (ít nhất 20 checklist) trong sandbox trước khi merge.
3.  **Agent Isolation**: Giữ cho Agent trong sandbox chỉ nhìn thấy các file liên quan đến nghiệp vụ đó để tránh "ảo giác" (hallucination) do quá tải context.

---

## 📊 3. TRỰC QUAN HÓA (Visual Tracking)

Quy tắc bắt buộc cho `task.md`:

-   **Codebase Graph**: Cập nhật sơ đồ liên kết giữa các file khi thêm logic mới.
-   **Task Dashboard**: Sử dụng Mermaid Gantt hoặc Flowchart để hiển thị trạng thái "đang chạy" của 20 sandbox.
-   **Checklist Status**: Mỗi sandbox PHẢI có file `SANDBOX_CHECKLIST.md` riêng để theo dõi tiến độ kiểm thử nội bộ.

---

## 🚨 4. QUY TẮC VÀNG (Golden Rules)

1.  **Đừng theo đuổi tối ưu AI**: Hãy theo đuổi việc tối ưu **Quy trình Planning**.
2.  **Vibe thủ**: Demo nhanh, nhưng scale phải chắc.
3.  **AI Agents**: Luôn đảm bảo Agent nắm đủ "DNA Nghiệp vụ" (từ `.shared/dna/`) trước khi giao task ở tầng Service phức tạp.

---

> 🔰 *Rule này định hướng cho mọi Agent hoạt động trong các hệ thống layered (.NET API + Vue Web) của dự án QuanLyDoanKham.*
