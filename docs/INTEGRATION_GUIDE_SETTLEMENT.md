# 📋 Hướng dẫn Tích hợp Module Quyết toán & Thống kê Nâng cấp

Tài liệu này hướng dẫn cách tích hợp các file mới vào dự án QuanLyDoanKham để nâng cấp module Quyết toán & Thống kê.

---

## 📁 Các File Mới Được Tạo

| File | Vị trí | Mô tả |
|------|--------|-------|
| `ReportingService_Enhanced.cs` | `/QuanLyDoanKham.API/Services/` | Backend service với logic tính toán cải thiện |
| `SettlementController.cs` | `/QuanLyDoanKham.API/Controllers/` | API endpoints cho Quyết toán |
| `MasterStatsDashboard.vue` | `/QuanLyDoanKham.Web/src/views/` | Giao diện hiển thị thống kê tổng hợp |

---

## 🔧 Các Bước Tích hợp

### Bước 1: Đăng ký Service trong Dependency Injection (Program.cs)

Mở file `QuanLyDoanKham.API/Program.cs` và thêm dòng sau vào phần `services.Add...`:

```csharp
// Thêm sau các service khác
services.AddScoped<IReportingServiceEnhanced, ReportingServiceEnhanced>();
```

**Vị trí chính xác:** Tìm dòng chứa `services.AddScoped<IReportingService, ReportingService>();` và thêm dòng mới ngay sau đó.

---

### Bước 2: Cập nhật Router Frontend

Mở file `QuanLyDoanKham.Web/src/router/index.js` (hoặc tương tự) và thêm route mới:

```javascript
{
  path: '/master-stats',
  component: () => import('../views/MasterStatsDashboard.vue'),
  meta: {
    title: 'Thống Kê Tổng Hợp',
    permission: 'Reports.View' // Hoặc quyền phù hợp
  }
}
```

---

### Bước 3: Thêm Menu Item (Tùy chọn)

Nếu bạn có file layout hoặc sidebar, thêm link tới trang mới:

```vue
<router-link to="/master-stats" class="menu-item">
  <BarChart3 class="w-5 h-5" />
  <span>Thống Kê Tổng Hợp</span>
</router-link>
```

---

## 📊 API Endpoints Mới

Sau khi tích hợp, bạn sẽ có 3 endpoint mới:

### 1. Lấy Chi tiết Quyết toán
```
GET /api/Settlement/detail/{healthContractId}
```
**Tham số:** `healthContractId` (int)
**Trả về:** `SettlementDetailDto`

**Ví dụ:**
```bash
curl "http://localhost:5000/api/Settlement/detail/1"
```

---

### 2. Lấy Thống kê Tổng hợp (Master Stats)
```
GET /api/Settlement/master-stats?startDate=2026-04-01&endDate=2026-04-15
```
**Tham số:**
- `startDate` (DateTime, tùy chọn) - Mặc định: 1 tháng trước
- `endDate` (DateTime, tùy chọn) - Mặc định: Hôm nay

**Trả về:** `MasterStatsDto`

**Ví dụ:**
```bash
curl "http://localhost:5000/api/Settlement/master-stats?startDate=2026-04-01&endDate=2026-04-15"
```

---

### 3. Lấy Danh sách Đối soát
```
GET /api/Settlement/reconciliation-list?startDate=2026-04-01&endDate=2026-04-15
```
**Tham số:**
- `startDate` (DateTime, tùy chọn)
- `endDate` (DateTime, tùy chọn)

**Trả về:** `List<ReconciliationItemDto>`

**Ví dụ:**
```bash
curl "http://localhost:5000/api/Settlement/reconciliation-list?startDate=2026-04-01&endDate=2026-04-15"
```

---

## 🔍 Các Cải thiện Chính

### 1. **Tính toán Doanh thu Chính xác**
- **Trước:** Chỉ tính doanh thu gói (Số ca × Đơn giá)
- **Sau:** Tính doanh thu gói + Doanh thu phát sinh (ExtraServiceRevenue)

### 2. **Chi phí Vật tư Thông minh**
- **Trước:** Tính OUT - IN chung chung từ StockMovement
- **Sau:** Tính theo định mức (10% doanh thu) hoặc thực tế nếu có gắn với hợp đồng

### 3. **Lợi nhuận Ròng Chính xác**
- **Trước:** Lợi nhuận = Doanh thu - (Lương + Vật tư + Overhead)
- **Sau:** Lợi nhuận = Doanh thu - (Lương + Vật tư + Overhead chia tỷ lệ)

### 4. **Danh sách Đối soát Tự động**
- Phát hiện ca khám **ngoài gói** (Quantity Variance > 0)
- Phát hiện ca khám **thiếu** (Quantity Variance < 0)
- Phát hiện ca khám **bị hủy** (Status = CANCELLED/REJECTED)

### 5. **Dashboard Master Stats**
- Hiển thị **Tổng doanh thu, Chi phí, Lợi nhuận** toàn hệ thống
- Biểu đồ **phân bổ chi phí** (Lương %, Vật tư %, Overhead %)
- Thống kê **nhân sự** (Tổng lượt điều động, Số nhân sự duy nhất)
- Cảnh báo **hợp đồng biên lợi nhuận thấp** (< 20%)

---

## ⚠️ Lưu ý Quan trọng

1. **Kiểm tra Database Schema**: Đảm bảo các bảng `MedicalRecords`, `Contracts`, `GroupStaffDetails`, `StockMovement`, `Overhead` tồn tại và có đầy đủ dữ liệu.

2. **Quyền truy cập**: Cấp quyền `Reports.View` hoặc `QuyetToan.View` cho người dùng muốn xem thống kê.

3. **Performance**: Nếu dữ liệu lớn, hãy thêm Index vào các bảng được query:
   ```sql
   CREATE INDEX idx_MedicalRecords_Status ON MedicalRecords(Status);
   CREATE INDEX idx_MedicalRecords_CheckInAt ON MedicalRecords(CheckInAt);
   CREATE INDEX idx_GroupStaffDetails_ExamDate ON GroupStaffDetails(ExamDate);
   ```

4. **Caching**: Xem xét thêm caching cho `GetMasterStatsAsync()` vì nó query nhiều bảng.

---

## 🧪 Kiểm tra Sau Tích hợp

1. **Chạy Backend:** `dotnet run` trong thư mục `QuanLyDoanKham.API`
2. **Chạy Frontend:** `npm run dev` trong thư mục `QuanLyDoanKham.Web`
3. **Truy cập:** `http://localhost:5173/master-stats` (hoặc port tương ứng)
4. **Kiểm tra API:** Sử dụng Postman hoặc curl để test các endpoint mới

---

## 📝 Tài liệu Tham khảo

- **ReportingService_Enhanced.cs**: Chứa logic tính toán chi tiết
- **SettlementController.cs**: Chứa API endpoints
- **MasterStatsDashboard.vue**: Chứa giao diện Frontend

---

*Hướng dẫn được tạo ngày 15/04/2026 bởi Manus AI.*
