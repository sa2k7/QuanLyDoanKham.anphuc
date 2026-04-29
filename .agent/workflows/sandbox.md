# Workflow: /sandbox - Quản lý Sandbox & Song song hóa

Workflow này giúp triển khai chiến thuật "10-20 Worktrees" và kiểm soát chất lượng bằng 20+ checklist song song.

## 🏁 Khi nào sử dụng?
- Khi bắt đầu một tính năng mới (Feature) hoặc sửa lỗi (Bugfix) trên hệ thống đa tầng.
- Khi muốn tách biệt môi trường để tránh ảnh hưởng đến nhánh chính.
- Khi cần chạy nhiều bài kiểm tra kiểm chứng nghiệp vụ cùng lúc.

---

## 🛠️ Bước 1: Khởi tạo Sandbox (`/sandbox create`)

1.  **Xác định scope**: Agent hỏi User tên task (ví dụ: `fix-calculate-price`).
2.  **Tạo Worktree**: Gọi script `manage-sandbox.ps1 create <task-name>`.
    - Tự động tạo thư mục sandbox tại `../sandboxes/<task-name>`.
    - Tự động kiểm tra nhánh source (thường là `main`).
3.  **Thiết lập Checklist**: Tạo file `SANDBOX_CHECKLIST.md` trong sandbox mới với ít nhất 20 đầu mục kiểm tra (Functional, UI, Backend, Security, State Machine).

---

## 🏗️ Bước 2: Planning & Visualizing

1.  **Cập nhật `task.md`**: Vẽ sơ đồ Mermaid thể hiện vị trí của Sandbox mới trong hệ thống.
2.  **Giao việc cho Agent**: Agent phụ trách sandbox đó PHẢI đọc `HEAVY-LAYERED-SYSTEM.md` và `dna_ref` tương ứng.

---

## 🧪 Bước 3: Kiểm soát song song (`/sandbox verify`)

1.  **Chạy checklist**: Thực hiện bài kiểm tra theo 20 đầu mục đã định nghĩa.
2.  **Báo cáo trạng thái**: Cập nhật Dashboard (Mermaid graph) trong `task.md` để User thấy tiến độ của tất cả sandbox đang chạy.
3.  **Audit**: Gọi workflow `/audit` riêng cho sandbox này trước khi merge.

---

## 🚢 Bước 4: Hợp nhất & Dọn dẹp (`/sandbox merge`)

1.  **Merge Git**: Thực hiện merge code từ sandbox vào nhánh chính.
2.  **Xóa sandbox**: Gọi script `manage-sandbox.ps1 delete <task-name>` để giải phóng tài nguyên.
3.  **Lưu trữ lỗi**: Ghi nhận bất kỳ "gotchas" nào vào `ERRORS.md`.

---

## 💡 Ví dụ lệnh
- `/sandbox create feature-oms`
- `/sandbox status` (Xem Dashboard trực quan)
- `/sandbox verify feature-oms`
