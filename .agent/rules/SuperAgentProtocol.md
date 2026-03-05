# Super Agent Protocol (High Autonomy)

Protocol này định nghĩa cách thức Agent vận hành trong chế độ tự động hóa hoàn toàn, loại bỏ tối đa các bước xác nhận thủ công (nút Run/Accept).

## 1. Nguyên tắc triển khai
- **Tiên phát chế nhân**: Agent chủ động phân tích và sửa đổi code ngay khi phát hiện vấn đề hoặc nhận yêu cầu mở rộng kiến trúc.
- **Khép kín quy trình**: Mỗi request từ người dùng phải được giải quyết trọn vẹn (Backend -> Frontend -> Migration -> Documentation) trong một luồng lệnh duy nhất.
- **Tự động sửa lỗi (Self-Healing)**: Khi lệnh `dotnet build` hoặc `npm run dev` báo lỗi, Agent phải coi đó là ưu tiên số 1 để giải quyết trước khi thông báo cho người dùng.

## 2. Kỹ thuật Tự động hóa (Technical Automation)
- **Bỏ qua xác nhận (Always Run)**: Luôn đặt `SafeToAutoRun: true` cho mọi lệnh `run_command` liên quan đến: build, test, migration, git, ls, cat, và các script tự động hóa.
- **Tự động tiếp tục (Auto Accept)**: Khi gửi `notify_user` sau khi hoàn thành task, luôn đặt `ShouldAutoProceed: true` để Agent có thể tiếp tục flow công việc mà không chờ người dùng nhấn "Duyệt".
- **Giao diện không chạm (Invisible UI)**: Hướng tới việc người dùng chỉ cần đưa ra yêu cầu cuối cùng, Agent sẽ tự động hóa mọi bước trung gian (Run/Accept/Approve). **Lưu ý**: Không tự động mở trình duyệt (browser) khi khởi chạy server trừ khi có yêu cầu cụ thể, chỉ cung cấp link URL cho người dùng.

## 3. Cơ chế Self-Approval
- `implementation_plan.md` sẽ được coi là "Đã phê duyệt" nếu Agent đảm bảo:
    - Không làm mất dữ liệu hiện có (luôn dùng Soft Delete hoặc Migrations).
    - Tuân thủ cấu trúc 3-layer (đối với Backend) và Composition API (đối với Frontend).
    - Có kế hoạch Verification cụ thể.

## 4. Quản lý trạng thái
- Agent duy trì `task.md` như một logbook thời gian thực.
- Sau mỗi cụm task thành công (ví dụ: xong CRUD 1 thực thể), Agent phải cập nhật walkthrough.

## 5. Trigger & Kích hoạt
- Khi người dùng sử dụng các từ khóa: "tự động", "làm hết đi", "quy trình khép kín", "tự phê duyệt", hoặc yêu cầu "tự động hóa nút Run/Accept".
- **Cam kết**: Chuyển đổi từ "Cộng tác viên" sang "Người thực thi độc lập".
