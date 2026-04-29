# 📊 MODULE_REPORTS: Bản đồ Tri thức Báo cáo

Bản đồ này chi tiết hóa luồng xử lý và cấu trúc của tính năng Xuất báo cáo và Thống kê.

## 📁 Danh sách tệp tin then chốt (Key Files)
- **Frontend**: `QuanLyDoanKham.Web/src/views/Reports.vue`
- **Controller**: `QuanLyDoanKham.API/Controllers/ReportsController.cs`
- **Services**: 
    - `IReportingService.cs` / `ReportingService.cs` (Dữ liệu thống kê).
    - `IReportExportService.cs` / `ReportExportService.cs` (Xuất file).
    - `FinancialReportService.cs` (Báo cáo tài chính).

## 🔄 Luồng xử lý (Processing Flow)
1. **Request**: UI yêu cầu xuất báo cáoDashboard (PDF/Excel) kèm khoảng thời gian.
2. **Controller**: `ReportsController` nhận yêu cầu, gọi `_exportService`.
3. **Logic**: `ReportExportService` sử dụng:
    - `QuestPDF` để tạo file PDF chuyên nghiệp.
    - `ClosedXML` để tạo file Excel.
4. **Data**: Tổng hợp dữ liệu từ nhiều nguồn qua `IReportingService`.

## 📈 Các loại báo cáo hỗ trợ
- **Dashboard Summary**: Xuất PDF/Excel tổng quan hoạt động.
- **Financial Report**: Báo cáo doanh thu, chi phí nhân sự và quyết toán.

## ⚠️ Lưu ý kỹ thuật (Technical Notes)
- File PDF được sinh ra dưới dạng `byte[]` và trả về qua `FileResult`.
- Các hàm export thường là `GenerateDashboardPdfAsync` và `GenerateDashboardExcelAsync`.
