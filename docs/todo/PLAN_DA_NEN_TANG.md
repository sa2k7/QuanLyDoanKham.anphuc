# Lộ trình phát triển Đa nền tảng (Android, iOS, PC, Laptop) cho QuanLyDoanKham

Dựa trên phân tích hiện trạng dự án sử dụng **Vue 3 (Vite)** và **ASP.NET Core (.NET 10)**, đây là kế hoạch chi tiết để đưa ứng dụng lên mọi nền tảng.

## 1. Giải pháp đề xuất: Capacitor + Electron
*   **Capacitor:** Dùng để đóng gói ứng dụng Web hiện tại thành ứng dụng Mobile (Android/iOS).
*   **Electron:** Dùng để đóng gói ứng dụng Web hiện tại thành ứng dụng Desktop (Windows/macOS/Linux).

## 2. Các giai đoạn triển khai

### Giai đoạn 1: Chuẩn bị Mobile (Android & iOS)
- [ ] **Cài đặt Capacitor:**
    ```bash
    cd QuanLyDoanKham.Web
    npm install @capacitor/core @capacitor/cli
    npx cap init QuanLyDoanKham com.yourdomain.qldk
    ```
- [ ] **Thêm nền tảng:**
    ```bash
    npm install @capacitor/android @capacitor/ios
    npx cap add android
    npx cap add ios
    ```
- [ ] **Cấu hình Camera & QR:** Cấp quyền truy cập camera trong `AndroidManifest.xml` và `Info.plist` để tính năng quét mã QR hoạt động trên mobile.

### Giai đoạn 2: Triển khai Desktop (PC/Laptop)
- [ ] **Tích hợp Electron:** Sử dụng `vite-plugin-electron` hoặc cài đặt Electron thủ công để đóng gói bản build Vite.
- [ ] **Tạo file cài đặt (.exe):** Sử dụng `electron-builder` để tạo bộ cài cho Windows.

### Giai đoạn 3: Tối ưu hóa UI/UX
- [ ] **Responsive Design:** Kiểm tra lại các bảng dữ liệu (Table) và Form khám bệnh trên màn hình điện thoại (sử dụng Tailwind CSS `sm:`, `md:`).
- [ ] **Offline Support:** Cân nhắc sử dụng Service Workers để ứng dụng vẫn có thể mở được khi mất mạng tạm thời.

### Giai đoạn 4: Cấu hình Backend (API)
- [ ] **CORS Policy:** Cập nhật `Program.cs` trong `QuanLyDoanKham.API` để cho phép các origin từ ứng dụng Mobile (`capacitor://localhost`, `http://localhost`).
- [ ] **HTTPS:** Đảm bảo API chạy trên HTTPS để Mobile có thể kết nối an toàn.

## 3. Ưu điểm của phương án này
1.  **Tiết kiệm 80% thời gian:** Không cần viết lại code bằng ngôn ngữ khác (như Java/Swift/Dart).
2.  **Duy trì dễ dàng:** Chỉ cần sửa code 1 nơi (thư mục Web), tất cả các nền tảng khác sẽ được cập nhật theo.
3.  **Tính năng đồng bộ:** Mọi dữ liệu khám bệnh được đồng bộ thời gian thực qua SignalR trên cả điện thoại và máy tính.

---
*Kế hoạch được tạo tự động bởi Manus dựa trên cấu trúc dự án hiện tại.*
