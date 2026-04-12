# 📄 ĐỀ XUẤT GIẢI PHÁP: HỆ THỐNG QUẢN LÝ ĐOÀN KHÁM SỨC KHỎE (MEDICAL GROUP MANAGEMENT)

**Người gửi**: Đội ngũ Phát triển AntiGravity (Agent: komi)
**Ngày đề xuất**: 10/04/2026
**Trạng thái**: Bản thảo sơ bộ (Draft for Review)

---

## 1. Đặt vấn đề (Problem Statement)

Hiện nay, quy trình quản lý các đoàn khám sức khỏe lưu động tại Phòng khám Đa khoa An Phúc đang đối mặt với các thách thức lớn:
- **Thao tác thủ công (Manual Overhead)**: Việc quản lý danh sách hàng ngàn nhân viên bằng Excel gây nhầm lẫn và tốn thời gian.
- **Gian lận chấm công**: Khó kiểm soát việc nhân sự có thực sự tham gia đoàn khám hay không khi làm việc tại các site ngoại tỉnh.
- **Sai sót quyết toán**: Việc tính lương dựa trên bảng chấm công giấy dẫn đến sai sót dòng tiền và khiếu nại từ nhân viên.
- **Thất thoát vật tư**: Chưa có hệ thống theo dõi vật tư tiêu hao (kim tiêm, ống nghiệm...) cho từng đoàn khám cụ thể.

## 2. Giải pháp kỹ thuật (Proposed Solution)

Chúng tôi đề xuất triển khai hệ thống **Digitized OMS (Operations Management System)** tích hợp trên nền tảng Web hiện đại:
- **Backend**: ASP.NET Core 8.0 (Hiệu năng cao, bảo mật, dễ mở rộng).
- **Frontend**: Vue.js 3 + Tailwind CSS (Giao diện trực quan, tương thích mọi thiết bị di động).
- **Cơ sở dữ liệu**: MS SQL Server (Đảm bảo an toàn dữ liệu và tính toàn vẹn).
- **Công nghệ lõi**: 
    - **QR Code HMAC**: Hệ thống chấm công bảo mật, ngăn chặn scan giả mạo.
    - **AI Dispatcher**: Tự động gợi ý nhân sự phù hợp cho từng vị trí dựa trên chuyên môn.

## 3. Mục tiêu dự án (Objectives)

1. **Số hóa 100%**: Không còn sử dụng giấy tờ trong khâu chấm công và báo cáo đoàn.
2. **Minh bạch hóa**: Dữ liệu lương và quyết toán được khóa sổ (Locking mechanism) để chống chỉnh sửa trái phép.
3. **Tối ưu nguồn lực**: Giảm 50% thời gian lập kế hoạch đoàn khám nhờ hệ thống tự động sinh (Auto-generate).
4. **Kiểm soát chi phí**: Theo dõi chính xác biên lợi nhuận (Gross Profit) từng hợp đồng theo thời gian thực.

## 4. Phạm vi triển khai (Scope)

Hệ thống bao gồm các Module cốt lõi:
- **Module Hợp đồng (Contracts)**: Quản lý vòng đời hợp đồng từ Nháp -> Đã duyệt -> Đang thực hiện.
- **Module Đoàn khám (Medical Groups)**: Điều hành các buổi khám thực địa.
- **Module Nhân sự & Chấm công (Attendance)**: Chấm công bằng QR Code và phê duyệt ca làm việc.
- **Module Tài chính (Payroll)**: Tự động tính lương tháng và xuất báo cáo nghiệm thu.
- **Module Vật tư (Supplies)**: Quản lý nhập/xuất kho cụ thể cho từng đoàn.

## 5. Ngân sách & Chi phí dự tính (Estimated Budget)

Dưới đây là bảng phân bổ chi phí dự kiến cho giai đoạn triển khai 6 tháng đầu tiên:

| Hạng mục | Chi tiết | Đơn giá (VND) | Thành tiền (VND) |
| :--- | :--- | :--- | :--- |
| **Hạ tầng (Infrastructure)** | Server SmarterASP, Domain, Vercel Pro | 500,000 / tháng | 3,000,000 |
| **Phát triển (Development)** | Xây dựng 5 module cốt lõi | 15,000,000 / module | 75,000,000 |
| **Tích hợp & Đào tạo** | Hướng dẫn bác sĩ, lễ tân sử dụng | 5,000,000 / gói | 5,000,000 |
| **Bảo trì (Maintenance)** | Hỗ trợ kỹ thuật 24/7, Backup dữ liệu | 2,000,000 / tháng | 12,000,000 |
| **TỔNG CỘNG** | | | **95,000,000** |

> [!NOTE]
> *Chi phí trên chỉ mang tính chất dự toán dựa trên quy mô SME (Doanh nghiệp vừa và nhỏ). Con số cuối cùng có thể thay đổi tùy thuộc vào mức độ tùy chỉnh Logic theo yêu cầu đặc thù của Phòng khám.*

## 6. Lợi ích mang lại (Benefits)

- **Về kinh tế**: Giảm lãng phí vật tư (~10-15%) và cắt giảm nhân sự hành chính làm bảng lương.
- **Về quản trị**: Lãnh đạo có thể theo dõi tiến độ đoàn khám ngay trên điện thoại di động mọi lúc, mọi nơi.
- **Về thương hiệu**: Nâng cao tính chuyên nghiệp trong mắt đối tác doanh nghiệp khi nghiệm thu bằng báo cáo số hóa chuẩn xác.

---

### Cam kết từ đội ngũ AntiGravity
Chúng tôi cam kết triển khai theo mô hình **Agile**, bàn giao từng phần để khách hàng có thể sử dụng ngay trong vòng **4 tuần đầu tiên**.
