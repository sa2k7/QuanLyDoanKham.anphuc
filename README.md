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

## 🛠 Công nghệ sử dụng (Tech Stack)

Hệ thống được phát triển theo mô hình **Client-Server**, sử dụng các công nghệ hiện đại:

### Backend
- **Node.js & Express.js:** Xây dựng máy chủ API mạnh mẽ, tốc độ cao.
- **MongoDB:** Hệ quản trị cơ sở dữ liệu NoSQL linh hoạt, lưu trữ thông tin không cấu trúc.

### Frontend
- **HTML5, CSS3, JavaScript:** Nền tảng giao diện cơ bản.
- **Thư viện UI/UX:** Hỗ trợ render linh hoạt, thiết kế theo hướng tiện dụng (Responsive Design).

---

## 📦 Thiết lập & Triển khai (Installation & Deployment)

Hệ thống được phân chia để triển khai trên các nền tảng đám mây khác nhau, đảm bảo hiệu năng và khả năng mở rộng.

### 1. Triển khai Backend (Trên Render)
Máy chủ API được lưu trữ trên Render. 
- Môi trường môi trường (Environment Variables) cần thiết lập trên Render:
  - `PORT`: Cộng dịch vụ (VD: `3000`)
  - `MONGO_URI`: Chuỗi kết nối tới cơ sở dữ liệu MongoDB.
- Quá trình deploy sẽ tự động chạy lệnh cài đặt package (`npm install`) và khởi động máy chủ (`npm start`).
- Sau khi deploy thành công, Render cung cấp một Domain API public.

### 2. Triển khai Frontend (Trên InterData - Plesk Control Panel)
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
3. **Phân công lịch khám:** Lên danh sách các bệnh viện/cơ sở khám tuyến dưới, chọn thời gian bắt đầu/kết thúc và gán nhân sự phù hợp từ danh sách.
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
