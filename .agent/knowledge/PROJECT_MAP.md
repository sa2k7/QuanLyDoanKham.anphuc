# 🗺️ PROJECT_MAP: Kiến trúc Hệ thống QuanLyDoanKham

Bản đồ này tổng hợp cấu trúc và các điểm neo quan trọng của hệ thống để AI truy xuất nhanh mà không cần đọc lại toàn bộ mã nguồn.

## 🏗️ Cấu trúc đa tầng (Multi-layered Architecture)

### 1. Frontend (QuanLyDoanKham.Web)
- **Framework**: Vue.js 3 + Vite.
- **Views**: `src/views/` (Vị trí các trang nghiệp vụ).
- **Components**: `src/components/` (Các thành phần UI dùng chung).
- **Core Views**:
    - `Reports.vue`: Quản lý danh sách báo cáo.
    - `SettlementReport.vue`: Trang quyết toán hợp đồng.

### 2. Backend (QuanLyDoanKham.API)
- **Framework**: ASP.NET Core (.NET 8).
- **Controllers**: `Controllers/` (Định nghĩa API Routes).
- **Business Logic**: `Services/` (Xử lý nghiệp vụ chính).
- **Data Access**: `Data/ApplicationDbContext.cs` (Entity Framework Core).
- **Entities**: `Models/` (Cấu trúc bảng Database).
- **DTOs**: `DTOs/` (Cấu trúc dữ liệu trao đổi API).

---

## 🔗 Luồng dữ liệu chính (Data Flows)

### Quyết toán (Settlement)
- **Frontend**: `SettlementReport.vue` 
- **API**: `SettlementController` (Cần xác minh qua grep)
- **Service**: `SettlementService` -> `CalculateSettlementAsync`
- **Tables**: `Contracts`, `MedicalGroups`, `GroupStaffDetails`.

### Báo cáo (Reporting)
- **Frontend**: `Reports.vue`
- **Service**: `FinancialReportService`, `ReportExportService`
- **Exports**: Hỗ trợ PDF và Excel (QuestPDF/ClosedXML).

---

## 📜 Quy chuẩn Code (Coding Standards)
- **Async/Await**: Sử dụng rộng rãi cho mọi thao tác I/O.
- **DTO pattern**: Không trả về Entity trực tiếp, luôn qua DTO.
- **Dependency Injection**: Quản lý Services qua Interface.
