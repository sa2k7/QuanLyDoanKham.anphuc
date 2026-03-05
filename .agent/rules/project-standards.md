# Project Standard & Autonomy Rules

## 1. Quyền tự chủ (Autonomy)
- **Tự động thực hiện**: Khi nhận được yêu cầu sửa lỗi hoặc thêm tính năng, Agent sẽ tự động phân tích toàn bộ các file liên quan, thực hiện sửa đổi (sử dụng `replace_file_content` hoặc `multi_replace_file_content`) mà không cần hỏi xác nhận từng bước.
- **Tự động sửa lỗi**: Nếu thấy lỗi Build hoặc Lint trong terminal, Agent phải chủ động đề xuất hoặc thực hiện sửa code ngay lập tức.
- **Không dùng Placeholder**: Không bao giờ để code dạng `// TODO` hoặc `// Logic ở đây`. Luôn viết code hoàn chỉnh.

## Phase 2: Continuous Execution (Action Second)
1. **Self-Approve**: If the analysis is complete and follows project standards, Agent will proceed to execution without waiting for explicit approval.
2. **Execute**: Implement the steps starting from the foundation (Backend/Database) up to the UI in a single continuous flow.
3. **Sync & Adjust**: Ensure consistency and fix any encountered errors on-the-fly.
4. **Verification**: Verify the implementation and update the walkthrough.

## 2. Tiêu chuẩn Backend (.NET)
- **DTOs**: Luôn đảm bảo các trường trong `CreateProductDto.cs` và các DTO khác khớp với yêu cầu nghiệp vụ và Database.
- **Async/Await**: Luôn sử dụng lập trình bất đồng bộ.
- **Validation**: Tự động thêm Data Annotations (`[Required]`, `[StringLength]`, v.v.) vào các DTO khi thấy cần thiết.

## 3. Tiêu chuẩn Frontend (Vue 3)
- **Composition API**: Luôn sử dụng `<script setup>`.
- **Naming**: File component dùng PascalCase, biến dùng camelCase.
- **Sync**: Khi Backend thay đổi API/DTO, phải chủ động kiểm tra và cập nhật Service/Store bên Vue.

## 4. Giao tiếp
- Thông báo ngắn gọn việc sẽ làm, sau đó "làm luôn".
- Giải thích "tại sao" sửa sau khi đã thực hiện xong.

## 5. Super Agent Mode (High Autonomy)
- **Tự động phê duyệt (Self-Approval)**: Agent có quyền tự phê duyệt các `implementation_plan.md` cho các tác vụ nâng cấp hệ thống, sửa lỗi hoặc tối ưu hóa mà Agent đánh giá là an toàn.
- **Thực thi liên tục**: Agent không cần chờ đợi xác nhận giữa các bước nhỏ nếu kế hoạch tổng thể đã được thiết lập.
- **Tự quản lý Task**: Tự động cập nhật `task.md` và `walkthrough.md` trong quá trình làm việc.
