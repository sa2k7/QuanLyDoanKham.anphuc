# BẢN KẾ HOẠCH ĐẠI TU TOÀN DIỆN DỰ ÁN QUẢN LÝ ĐOÀN KHÁM (OVERHAUL BLUEPRINT)
**Vai trò:** Lead Software Architect & Healthcare Expert
**Mục tiêu:** Nâng cấp hệ thống lên tiêu chuẩn y tế chuyên nghiệp, đảm bảo tính toàn vẹn nghiệp vụ (Business Integrity), bảo mật và hiệu suất cao.

---

## 1. PHÂN TÍCH HIỆN TRẠNG & CÁC LỖ HỔNG NGHIỆP VỤ (GAP ANALYSIS)

Dựa trên việc rà soát mã nguồn hiện tại, hệ thống đang gặp phải các vấn đề cốt lõi sau cần được giải quyết triệt để:

### 1.1. Tầng Dữ liệu & Bảo mật
| Lỗ hổng hiện tại | Hệ quả | Giải pháp Đại tu |
| :--- | :--- | :--- |
| **Thiếu Database Constraints** | Dữ liệu rác (ví dụ: gán Bác sĩ làm Tiếp tân) có thể lọt vào DB qua các script ngoài hoặc lỗi code. | Áp dụng **Check Constraints** và **Triggers** ở mức SQL Server/PostgreSQL. |
| **Giao dịch (Transactions) rời rạc** | Khi lỗi xảy ra ở bước sau (ví dụ: gán nhân sự), bước trước (tạo đoàn) không được rollback hoàn toàn hoặc thiếu nhất quán. | Triển khai **Unit of Work** kết hợp **EF Core Transactions** cho mọi luồng tài chính/điều phối. |
| **Phân quyền chưa chặt chẽ** | Một số API mới chỉ dừng ở mức `[Authorize]`, chưa kiểm tra Permission cụ thể cho từng Action. | Phủ sóng `[AuthorizePermission]` toàn diện và tích hợp vào Middleware. |

### 1.2. Tiếp đón & Điều phối Bệnh nhân
| Lỗ hổng hiện tại | Hệ quả | Giải pháp Đại tu |
| :--- | :--- | :--- |
| **Luồng Check-in thủ công** | Tốc độ xử lý tại trạm Check-in chậm, dễ gây ùn tắc khi đoàn khám đông (500+ người). | **QR Check-in tự động**: Quét mã -> Tự động sinh hàng đợi (Queue) dựa trên `Station.SortOrder`. |
| **Trạng thái Real-time yếu** | Nhân viên trạm sau không biết bệnh nhân đã xong trạm trước hay chưa trừ khi tải lại trang. | Tích hợp **SignalR** để đồng bộ trạng thái `MedicalRecord` tức thời giữa các trạm. |
| **Hàng đợi (Queue) tĩnh** | Chưa có logic điều phối thông minh dựa trên độ tải (Load Balancing) của từng trạm. | Xây dựng bộ não điều phối Queue dựa trên `WipLimit` của từng Station. |

### 1.3. Nhân sự & Tài chính (P&L)
| Lỗ hổng hiện tại | Hệ quả | Giải pháp Đại tu |
| :--- | :--- | :--- |
| **Gán nhân sự cảm tính** | Có thể gán trùng lịch cho một nhân sự ở 2 đoàn khác nhau cùng ngày. | **Clinical Matrix Validation**: Chặn gán trùng lịch và kiểm tra chứng chỉ hành nghề (CCHN) phù hợp vị trí. |
| **Tính toán chi phí cứng nhắc** | Chi phí vật tư (Supply) và chi phí chung (Overhead) chưa được trừ kho thực tế theo từng ca khám. | **Dynamic Calculator**: Tự động trừ kho theo định mức vật tư tiêu hao thực tế của từng trạm. |
| **Báo cáo P&L sơ sài** | Chỉ hiển thị doanh thu/chi phí thô, chưa thấy được độ lệch (Variance) và hiệu suất thực tế. | **P&L Control Center**: Dashboard hiển thị lãi/lỗ ròng, độ lệch vật tư và hiệu suất nhân sự. |

---

---

## 2. KIẾN TRÚC TẦNG DỮ LIỆU & BẢO MẬT (SQL, EF CORE, PERMISSION)

### 2.1. Triển khai EF Core Transactions & Unit of Work
Để đảm bảo **Business Integrity**, mọi thao tác tài chính (Quyết toán, Gán lương) và điều phối (Tạo đoàn, Gán nhân sự) phải được bọc trong một **Transaction**.

```csharp
// Mẫu triển khai trong BaseService hoặc Repository
using (var transaction = await _context.Database.BeginTransactionAsync())
{
    try {
        // 1. Cập nhật trạng thái Đoàn khám
        // 2. Gán nhân sự theo định mức (Quotas)
        // 3. Khởi tạo hàng đợi cho bệnh nhân
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    } catch (Exception) {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### 2.2. Áp dụng Ràng buộc cứng (Database Constraints)
Triển khai trong `OnModelCreating` của `ApplicationDbContext` để ngăn chặn gán sai vai trò lâm sàng:

```csharp
// Ngăn gán Bác sĩ (StaffType = 'BacSi') vào các vị trí Hành chính (PositionCode = 'CHECKIN', 'RECEPTION')
modelBuilder.Entity<GroupStaffDetail>()
    .ToTable(t => t.HasCheckConstraint("CK_Staff_Clinical_Role", 
        "(PositionCode NOT IN ('CHECKIN', 'RECEPTION') OR StaffType != 'BacSi')"));

// Chặn gán trùng lịch làm việc (Unique Index trên StaffId và ExamDate)
modelBuilder.Entity<GroupStaffDetail>()
    .HasIndex(g => new { g.StaffId, g.ExamDate })
    .IsUnique()
    .HasFilter("[WorkStatus] != 'Absent'"); // Chỉ chặn nếu không phải trạng thái Nghỉ
```

### 2.3. Bảo mật toàn diện với [AuthorizePermission]
Thay thế `[Authorize]` cơ bản bằng Attribute phân quyền chi tiết cho từng Controller:

| Controller | Permission Required | Mục đích |
| :--- | :--- | :--- |
| `HealthContractsController` | `HopDong.View`, `HopDong.Approve` | Xem và phê duyệt hợp đồng tài chính. |
| `MedicalGroupsController` | `DoanKham.Manage`, `DoanKham.Assign` | Quản lý đoàn và gán nhân sự. |
| `SettlementController` | `QuyetToan.Calculate`, `QuyetToan.Finalize` | Tính toán và chốt quyết toán. |
| `ReportsController` | `BaoCao.ViewFinance`, `BaoCao.ViewOperational` | Truy cập dữ liệu nhạy cảm về P&L. |

---

---

## 3. THIẾT KẾ LUỒNG NGHIỆP VỤ Y TẾ CHUYÊN SÂU (CLINICAL WORKFLOW)

### 3.1. Luồng QR Check-in Tự động (Queue Engine)
Nâng cấp logic từ `CheckInService` để tự động khởi tạo lộ trình khám cho bệnh nhân ngay khi quét mã:

1.  **Quét mã QR**: Giải mã `QrToken` lấy `MedicalRecordId`.
2.  **Khởi tạo StationTasks**: Dựa trên gói khám của Hợp đồng, tự động tạo các bản ghi `RecordStationTask` cho bệnh nhân.
3.  **Sắp xếp Hàng đợi (Sorting)**: Lấy tất cả các trạm cần khám, sắp xếp theo `Station.SortOrder`.
4.  **Phát sinh Số thứ tự (QueueNo)**: Cấp số thứ tự theo ca khám (Group) để điều phối tại các trạm.
5.  **Cập nhật Trạng thái Real-time**: Gửi SignalR tới trạm đầu tiên (thường là CHECKIN hoặc SINH_HIEU).

### 3.2. Clinical Matrix & Tự động hóa Nhân sự (GroupStaffDetail)
Xây dựng ma trận gán nhân sự dựa trên định mức trạm (`GroupPositionQuotas`):

*   **Logic gán tự động**:
    *   **Input**: Số lượng bệnh nhân dự kiến (`ExpectedQuantity`).
    *   **Output**: Danh sách nhân sự tối ưu dựa trên **Clinical Matrix** (StaffType vs Position).
    *   **Ràng buộc**: `Staff.IsActive = true` và không trùng `ExamDate`.
*   **Đồng bộ Quota**:
    *   Mọi thay đổi trong `GroupStaffDetail` phải cập nhật lại trạng thái của `GroupPositionQuotas` (Đủ/Thiếu nhân sự).

### 3.3. Bộ máy tính toán tài chính (Dynamic Calculator)
Xây dựng `FinancialCalculatorEngine` để tổng hợp dữ liệu quyết toán:

| Thành phần chi phí | Logic tính toán | Nguồn dữ liệu |
| :--- | :--- | :--- |
| **Chi phí Lương (Labor)** | `Staff.DailyRate * ShiftType` (theo ca khám thực tế). | `GroupStaffDetail` |
| **Vật tư (Material)** | `SupplyItem.CostPrice * Quantity` (trừ kho theo thực tế ca khám). | `StockMovement` (OUT) |
| **Chi phí chung (Overhead)** | Phân bổ theo hợp đồng hoặc định mức cố định. | `Overhead` |
| **Doanh thu (Revenue)** | `MedicalRecord.Count(COMPLETED) * Contract.UnitPrice`. | `MedicalRecord` |
| **Thuế & Chiết khấu** | Áp dụng `VATRate` và `DiscountPercent` động từ hợp đồng. | `HealthContract` |

---

---

## 4. ĐỊNH HƯỚNG TRẢI NGHIỆM NGƯỜI DÙNG & GIAO DIỆN (UI/UX)

### 4.1. Thống nhất phong cách Premium Glassmorphism
Nâng cấp giao diện từ Vue.js hiện tại sang phong cách **Glassmorphism** chuyên nghiệp:

*   **Header & Sidebar**: Sử dụng `backdrop-blur-md` và `bg-white/70` để tạo hiệu ứng trong suốt.
*   **Cards & Widgets**: Bo góc `rounded-3xl`, viền mỏng `border-white/20`, đổ bóng mềm `shadow-glass`.
*   **Typography**: Sử dụng font chữ hiện đại, tập trung vào tính dễ đọc của các chỉ số y khoa.

### 4.2. Dashboard P&L - Trung tâm điều khiển tài chính
Xây dựng trang thống kê thành một **Control Center** thực thụ:

| Widget | Chức năng | Chỉ số chính |
| :--- | :--- | :--- |
| **P&L Summary Card** | Tổng quan tài chính. | Lãi/Lỗ ròng (Net Profit), Biên lợi nhuận (Margin). |
| **Labor Efficiency Chart** | Hiệu suất nhân sự. | Số bệnh nhân khám/nhân viên, Chi phí lương/doanh thu. |
| **Material Variance** | Độ lệch vật tư. | So sánh định mức ban đầu vs Thực tế hao phí tại trạm. |
| **Contract Progress** | Tiến độ hợp đồng. | Số hồ sơ COMPLETED / Dự kiến, Doanh thu đã ghi nhận. |

### 4.3. Đồng bộ trạng thái Real-time (SignalR)
Tích hợp **SignalR** vào Frontend để đồng bộ hóa:

*   **Queue Dashboard**: Tự động cập nhật danh sách chờ khi bệnh nhân check-in.
*   **Station Coordinator**: Cập nhật tức thời trạng thái hồ sơ (`MedicalRecord`) từ các trạm khám.
*   **Notifications**: Thông báo lỗi nghiệp vụ (`ServiceResult`) rõ ràng và yêu cầu xác nhận logic cho các thao tác quan trọng (Phê duyệt, Quyết toán).

---

---

## 5. LỘ TRÌNH THỰC HIỆN (ROADMAP)

### Giai đoạn 1: Gia cố nền tảng (Database & Security)
*   **Thời gian**: 1-2 tuần.
*   **Công việc**: Áp dụng EF Core Migrations cho Constraints, Indexes và tích hợp `AuthorizePermission` Attribute.

### Giai đoạn 2: Tự động hóa nghiệp vụ (Clinical Logic)
*   **Thời gian**: 2-3 tuần.
*   **Công việc**: Xây dựng `QueueEngine` cho QR Check-in và `ClinicalMatrix` cho gán nhân sự.

### Giai đoạn 3: Tính toán tài chính & Báo cáo (P&L Engine)
*   **Thời gian**: 2-3 tuần.
*   **Công việc**: Phát triển `FinancialCalculatorEngine` và thiết kế Dashboard P&L Control Center.

### Giai đoạn 4: Trải nghiệm người dùng (Premium UI/UX)
*   **Thời gian**: 1-2 tuần.
*   **Công việc**: Thống nhất Layout Glassmorphism và tích hợp SignalR cho Real-time sync.

---
**Ghi chú:** Bản kế hoạch này tập trung vào **Business Integrity** (Tính toàn vẹn của nghiệp vụ) để giải quyết triệt để tình trạng logic rời rạc hiện tại của dự án QuanLyDoanKham.
