# PHẦN 6 - KIỂM THỬ

## 6.1. Mục tiêu kiểm thử
Mục tiêu của quá trình kiểm thử là đảm bảo hệ thống "Quản Lý Đoàn Khám" hoạt động đúng yêu cầu đã đề ra, không phát sinh lỗi nghiêm trọng, xử lý mượt mà và mang lại trải nghiệm tốt nhất cho người dùng. Các mục tiêu chính bao gồm:
- **Xác minh chức năng:** Đảm bảo tính đúng đắn của các nghiệp vụ quan trọng như quản lý đoàn khám, đăng ký lịch khám, phân công bác sĩ, và theo dõi kết quả khám bệnh.
- **Tính nhất quán UI/UX:** Đảm bảo giao diện theo phong cách Brutalist hiển thị nhất quán, không vỡ layout trên nhiều độ phân giải màn hình và trình duyệt khác nhau.
- **Hiệu năng và tải trọng:** Kiểm tra khả năng đáp ứng của hệ thống khi có lượng lớn người dùng (ví dụ: các đoàn khám lớn với hàng trăm bệnh nhân, import Excel số lượng lớn).
- **Tính bảo mật:** Đảm bảo an toàn dữ liệu bệnh nhân, phân quyền truy cập chặt chẽ (Admin, Bác sĩ, Nhân viên, Quản lý đoàn khám).

## 6.2. Phạm vi kiểm thử
Phạm vi kiểm thử bao trùm toàn bộ các thành phần của hệ thống:
- **Frontend (Vue.js 3, TailwindCSS):** Giao diện danh sách đoàn khám, quản lý nhân sự, lịch khám, hồ sơ bệnh nhân, form nhập liệu, xuất/nhập file Excel.
- **Backend (ASP.NET Core Web API, SQL Server):** Tính toàn vẹn của database, các endpoint API xử lý dữ liệu đoàn khám, lịch trình, kết quả khám, thuật toán AI gợi ý.
- **Tích hợp (Integration):** Kiểm tra kết nối luồng dữ liệu đồng bộ giữa Frontend và Backend (luồng đăng ký, xác nhận, cập nhật trạng thái hồ sơ).
- **Bảo mật (Security):** Kiểm tra tính hợp lệ của JWT Token, chống xâm nhập trái phép, kiểm soát quyền truy cập theo Role.
- **Hiệu năng (Performance):** Tốc độ phản hồi API, thời gian render giao diện khi tải danh sách hàng ngàn mẫu dữ liệu.

## 6.3. Kế hoạch kiểm thử
Quá trình kiểm thử được lên kế hoạch và thực hiện theo từng giai đoạn cụ thể:

| Giai đoạn | Thời gian | Mô tả chi tiết |
| :--- | :--- | :--- |
| **Chuẩn bị Test Case** | 02/02 – 04/02 | Xác định các kịch bản kiểm thử (Test Scenarios), viết bảng Test Case chi tiết cho từng luồng nghiệp vụ. |
| **Kiểm thử chức năng** | 05/02 – 15/02 | Thực hiện chạy các Test Case độc lập trên từng Unit của Frontend (Components) và Backend (Controllers/Services). |
| **Kiểm thử tích hợp** | 16/02 – 19/02 | Kết nối hoàn chỉnh hệ thống FE-BE, kiểm tra luồng dữ liệu xuyên suốt (từ lúc tạo đoàn khám đến khi trả kết quả). |
| **Kiểm thử hệ thống** | 20/02 – 22/02 | Chạy thử nghiệm toàn bộ hệ thống (End-to-End) với một lượng lớn dữ liệu giả lập (Mock Data). |
| **Hoàn thiện báo cáo** | 23/02 | Tổng hợp kết quả, báo cáo các lỗi tồn đọng, chụp ảnh minh họa làm minh chứng. |

## 6.4. Bảng kiểm thử (Test Cases)
Danh sách các Test Case chi tiết và kết quả thực hiện được ghi nhận đầy đủ nhằm đảm bảo độ bao phủ (test coverage). Nội dung chi tiết các ca kiểm thử có thể tham khảo trực tiếp tại:
👉 [**Bảng Test Case QuanLyDoanKham (Google Sheets)**](https://docs.google.com/spreadsheets/d/1pqxa72trRZuD89L71dA_fImS0178GPuhQ5OhPJgNve4/edit?usp=sharing)

*(Ghi chú: Đã điều chỉnh thông tin Backend từ Node.js/MongoDB thành .NET Core/SQL Server cho đúng với kiến trúc mã nguồn thực tế của dự án.)*
