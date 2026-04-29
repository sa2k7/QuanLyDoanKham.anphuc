---
trigger: always_on
---

# SAFE-DEVELOPMENT-WORKFLOW.MD - Giao Thức Code An Toàn

> **Mục tiêu**: Đảm bảo AI (Komi) tự động áp dụng các quy tắc an toàn bảo vệ CSDL Production và thiết lập quy trình kiểm thử trước khi tung lên hệ thống (CI/CD) mà không cần người dùng ra lệnh lại.

---

## 🚦 1. PRE-FLIGHT CHECK (KIỂM TRA TRƯỚC VÀO VIỆC)

MỖI LẦN NHẬN MỘT YÊU CẦU MỚI TỪ USER, Agent PHẢI âm thầm và TỰ ĐỘNG thực hiện các kiểm tra sau VÀ báo cáo vắn tắt ở đầu câu trả lời:

1. **Kiểm tra trạng thái máy chủ ảo (Local vs Prod):**
   - Xác minh xem `appsettings.json` đang nối với LocalDB (môi trường Test) hay là SmarterASP (Production `sql5111.site4now.net`).
   - Nếu đang ở hàm Production, Agent PHẢI cẩn trọng cao độ khi thực thi các lệnh migration (`ef update`).

2. **Chụp ảnh Git (Git Status Snapshot):**
   - Tự động chạy `git status` ngầm để xem còn code rác/hoặc code cũ chưa được `commit` không.
   - Tránh hiện tượng ghi đè làm mất công sức làm việc.

---

## 🛠️ 2. QUY TRÌNH THỰC THI (SAFE EXECUTION)

Trong lúc thực thi Code hoặc chỉnh sửa Giao Diện (Frontend), Agent PHẢI tuân thủ:

1. **Đẩy thẳng Lên Mạng Chính (Push to Main):**
   - Theo chỉ định mới nhất của Sếp, mọi thay đổi Giao Diện (Vercel) sẽ được phép `push` thẳng lên nhánh `main` để Live Site cập nhật ngay lập tức.
   - Bỏ qua các khâu tạo nhánh phụ (Preview URLs) rườm rà. Sửa xong -> Vercel chạy thật luôn!

2. **Snapshot định kỳ (Thói quen Commit):**
   - Tự động nhắc nhở (hoặc tự động gọi) `git add` & `git commit` mỗi khi sửa xong 1 File quan trọng hoặc vượt qua 1 bài Test thành công. Không bao giờ gộp cả 10 tính năng vào đúng 1 cú commit lười biếng.

3. **Check Backup Database:**
   - TRƯỚC KHI sử dụng lệnh Xóa bảng, Migrate Schema xuống Entity Framework... Agent PHẢI nhắc User chốt chặng: *"Bạn đã đăng nhập SmarterASP bấm Backup CSDL của ngày hôm nay chưa? Khi nào Backup xong thì báo để mình bung lệnh Database Update nhé!"*.

## 🐚 3. QUY TẮC KÝ TỰ ĐẶC BIỆT (SHELL ESCAPING)

Để tối ưu hóa và ngăn chặn các lỗi lặp lại do can thiệp của tầng Shell:

1. **Cấm truyền chuỗi nhạy cảm qua CMD**: 
   - Tuyệt đối KHÔNG truyền các chuỗi có ký tự đặc biệt (`$`, `&`, `|`, v.v.) như BCrypt Hash, API Key, Token trực tiếp qua tham số dòng lệnh (`sqlcmd -Q "..."`).
   - Mọi thao tác cập nhật dữ liệu nhạy cảm PHẢI được thực hiện thông qua tệp tin trung gian (ví dụ: `.sql`, `.ps1`, `.js`) rồi gọi thực thi tệp đó.

2. **Xác minh sau thực thi**: 
   - Sau mỗi lệnh Update dữ liệu quan trọng, Agent PHẢI thực hiện lệnh `SELECT` kiểm tra lại độ dài (`LEN()`) và tính toàn vẹn của dữ liệu vừa cập nhật.

---

> 🔰 Với Rule này, Komi sẽ tự mang trên mình tư duy Của một Senior DevOps: "Live Site là Mạng Sống - Code phải Test an toàn". User vĩnh viễn không cần nhắc lại giao thức tự phòng vệ này.
