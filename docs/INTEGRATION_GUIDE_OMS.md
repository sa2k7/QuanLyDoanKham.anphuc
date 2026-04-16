# 📋 Hướng dẫn Tích hợp Module Điều phối & Tiếp đón Nâng cấp

Tài liệu này hướng dẫn cách tích hợp các thành phần mới để tự động hóa hàng đợi và tối ưu quy trình tiếp đón.

---

## 📁 Các File Mới Được Tạo

| File | Vị trí | Mô tả |
|------|--------|-------|
| `SmartQueueService.cs` | `/QuanLyDoanKham.API/Services/MedicalRecords/` | Thuật toán điều phối thông minh |
| `OmsController_Enhanced.cs` | `/QuanLyDoanKham.API/Controllers/` | API endpoints cho Điều phối & Self Check-in |
| `QueueMonitor.vue` | `/QuanLyDoanKham.Web/src/views/` | Màn hình gọi số công cộng (Public Display) |

---

## 🔧 Các Bước Tích hợp

### Bước 1: Đăng ký Service trong Dependency Injection (Program.cs)

Mở file `QuanLyDoanKham.API/Program.cs` và thêm dòng sau:

```csharp
services.AddScoped<ISmartQueueService, SmartQueueService>();
```

---

### Bước 2: Cập nhật Router Frontend

Mở file `QuanLyDoanKham.Web/src/router/index.js` và thêm route cho màn hình gọi số:

```javascript
{
  path: '/queue-monitor',
  name: 'QueueMonitor',
  component: () => import('../views/QueueMonitor.vue'),
  meta: {
    title: 'Màn Hình Gọi Số Công Cộng',
    layout: 'empty' // Sử dụng layout không có sidebar/header để hiển thị full màn hình
  }
}
```

---

### Bước 3: Tích hợp Thuật toán Gợi ý vào State Machine

Để hệ thống tự động gợi ý trạm tiếp theo ngay sau khi Check-in hoặc Hoàn thành một trạm, bạn hãy chỉnh sửa `MedicalRecordStateMachine.cs`:

Trong phương thức `CheckInAsync` hoặc `CompleteStationAsync`, gọi `_smartQueueService.SuggestNextStationAsync(recordId)` để lấy mã trạm tiếp theo và trả về cho Frontend.

---

## 📊 Các Tính năng Mới

### 1. **Thuật toán Hàng đợi Thông minh (Smart Queue)**
Hệ thống không còn bắt bệnh nhân đi theo một lộ trình cứng nhắc. Thuật toán sẽ tính toán trạm tiếp theo dựa trên:
- **Tải hiện tại:** Trạm nào đang ít người chờ nhất.
- **Thứ tự ưu tiên:** Các trạm cơ bản (Cân đo, Huyết áp) vẫn được ưu tiên trước nếu vắng.
- **Trạng thái khám:** Chỉ gợi ý các trạm bệnh nhân chưa khám (WAITING).

### 2. **Màn Hình Gọi Số Công Cộng (Queue Monitor)**
Giao diện mới dành cho TV tại sảnh chờ hoặc trước các phòng khám:
- Hiển thị danh sách các trạm đang hoạt động.
- Hiển thị số thứ tự đang khám (Serving Now) và chuẩn bị khám (Up Next).
- Cập nhật thời gian thực qua SignalR (không cần F5 trang).
- Có thanh thông báo chạy chữ (News Ticker) bên dưới.

### 3. **Hỗ trợ Self Check-in (Báo danh tự động)**
API mới `api/OmsControllerEnhanced/self-checkin` cho phép xây dựng các Kiosk để bệnh nhân tự nhập mã hồ sơ hoặc quét QR mà không cần nhân viên y tế hỗ trợ.

---

## 🔍 Cách Truy cập Màn Hình Gọi Số

Bạn có thể mở màn hình gọi số cho một đoàn khám cụ thể bằng cách thêm tham số `groupId`:
`http://localhost:5173/queue-monitor?groupId=10`

---

## 📝 Lưu ý Vận hành

1. **TV/Màn hình lớn**: Màn hình gọi số được thiết kế tối ưu cho độ phân giải Full HD (1920x1080) và chế độ Dark Mode để bệnh nhân dễ quan sát từ xa.
2. **SignalR**: Đảm bảo Backend Hub đang hoạt động ổn định để các con số trên màn hình được nhảy tự động khi bác sĩ bấm "Bắt đầu khám".
3. **In phiếu**: Trong `CheckIn.vue`, hãy kích hoạt hàm `printReceipt` để in phiếu số thứ tự ngay sau khi bệnh nhân báo danh thành công.

---

*Hướng dẫn được tạo ngày 15/04/2026 bởi Manus AI.*
