# 🏥 HỆ THỐNG QUẢN LÝ NHÂN SỰ ĐOÀN KHÁM NGOẠI TUYẾN 

Hệ thống quản lý đoàn khám bệnh phòng ngoại tuyến của Phòng Khám Đa Khoa An Phúc Sài Gòn. Đây là giải pháp phần mềm giúp số hóa và tối ưu hóa quy trình phân công, điều phối nhân sự y tế.

---

## 🚀 Giới thiệu (Overview)

Việc tổ chức các đoàn khám bệnh lưu động đòi hỏi sự phối hợp chặt chẽ giữa nhiều bộ phận: bác sĩ, điều dưỡng, tình nguyện viên và bộ phận hỗ trợ. Quản lý thủ công gây ra nhiều khó khăn trong việc theo dõi lịch trình và đồng bộ thông tin. 

Dự án này cung cấp một nền tảng Web-based quản trị tập trung giúp:
- 📊 **Quản lý thông tin nhân sự:** Dễ dàng theo dõi, thêm, sửa, xóa thông tin bác sĩ, điều dưỡng.
- 📅 **Phân công nhiệm vụ rành mạch:** Lên lịch trình, sắp xếp nhân sự vào đúng chuyên môn cho mỗi đoàn khám.
- ⚡ **Theo dõi thời gian thực:** Nắm bắt lịch trình và trạng thái tham gia của từng thành viên.
- 📈 **Tự động hóa:** Giảm thiểu sai sót do điều phối thủ công, tối ưu hóa hoạt động nguồn nhân lực.

---

## 🛠 Công nghệ sử dụng (Tech Stack - Current v4.0)

Hệ thống đã được nâng cấp lên kiến trúc Enterprise:

### Backend
- **ASP.NET Core 10.0 (Web API):** Hiệu suất cao, bảo mật chặt chẽ.
- **Entity Framework Core:** Quản lý CSDL SQL Server chuyên nghiệp.
- **SQL Server (Local/SmarterASP):** Lưu trữ dữ liệu quan hệ tối ưu.

### Frontend
Giao diện người dùng được lưu trữ trên InterData thông qua Plesk.
- Chỉ định đường dẫn API từ Backend trong mã nguồn Frontend trước khi đóng gói:
```javascript
const API_BASE_URL = "https://<your-backend-render-domain>.onrender.com";
```
- Sử dụng Plesk File Manager để tải lên thư mục mã nguồn Frontend (hoặc `dist`/`build` file tùy thuộc cấu hình).
- Trỏ domain của Frontend về thư mục gốc (`public_html` hoặc `HTDOCS`).

---

## 📖 Hướng dẫn sử dụng (Usage Guide)

Quy trình sử dụng cơ bản dành cho Quản trị viên (Admin) và Nhân viên:

1. **Đăng nhập Hệ thống:** Truy cập vào Domain Frontend đã cấu hình. Nhập thông tin tài khoản được cấp.
2. **Quản lý User nhân sự:** Điều hướng đến mục quản lý để thêm hồ sơ bác sĩ/nhân viên, gán vai trò tương ứng.
3. **Phân công lịch khám:** Lên danh sách các bệnh viện/cơ sở khám tuyến dưới, chọn thời gian bắt đầu/kết thúc

## 🛡️ Giao thức Vận hành (Komi Protocol)

- **Local-First:** Tất cả tính năng Backend API PHẢI được chạy và test ổn định 100% tại máy nhánh (`localhost`) trước khi nghĩ đến chuyện đẩy lên Production.
- **Quy trình FE (Vercel):** Theo chỉ định mới nhất, mọi thay đổi Giao Diện (Vercel) sẽ được phép push thẳng lên nhánh `main` để Live Site cập nhật tự động (Bỏ qua nhánh Preview rườm rà).
- **Ghi nhớ lỗi:** Mọi lỗi logic phát hiện được phải được lưu vào `ERRORS.md` để Agent "Komi" tự động học hỏi và không bao giờ lặp lại lỗi cũ.

4. **Theo dõi tiến độ:** Các thành viên trong đoàn có thể xem chi tiết lịch trình của mình để chuẩn bị. Admin có cái nhìn tổng quan về tất cả các đoàn khám đang diễn ra.

---

## 👥 Nhóm phát triển (The Team)

Dự án được thực hiện bởi **Nhóm 71 - Trường Cao Đẳng FPT Polytechnic (Đồng Nai)**, bao gồm các thành viên:

- 👨‍💻 **Trịnh Trung Hiếu** (TB00606)
- 👨‍💻 **Lê Nguyễn Thanh Bình** (TB00310)
- 👨‍💻 **Phạm Minh Thiện** (TB00442)
- 👨‍💻 **Võ Trần An Sang** (TB00626)
- 👨‍💻 **Nguyễn Phúc Minh** (TB0085)

*Giáo viên hướng dẫn:* **Nguyễn Hữu Thiên Ân**

---

## 📌 Kết luận & Hướng phát triển (Conclusion & Future Work)

### Thuận lợi & Khó khăn  
- **Thuận lợi:** Nắm bắt được nhu cầu thực tế từ cơ sở y tế. Áp dụng các công nghệ Web đang phổ biến, cộng đồng hỗ trợ lớn.
- **Khó khăn:** Quản lý đồng bộ dữ liệu phức tạp do tính chất đặc thù của lịch trình lưu động. Cần tối ưu thời gian phản hồi giao diện.

### Hướng phát triển tương lai
- 🔔 Tích hợp hệ thống thông báo (Notification/Email/SMS) nhắc nhỏ lịch trình cho bác sĩ.
- 📱 Phát triển ứng dụng di động (Mobile App) để nhân viên y tế dễ dàng cập nhật trạng thái mọi lúc mọi nơi.
- 🛡️ Nâng cấp tính năng định danh (Authentication/Authorization) bảo mật cấp độ cao hơn.
- 📊 Tích hợp Analytics Dashboard để phân tích hiệu suất làm việc của cán bộ y tế.
