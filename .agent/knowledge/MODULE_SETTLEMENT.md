# 💳 MODULE_SETTLEMENT: Bản đồ Tri thức Quyết toán

Bản đồ này chi tiết hóa luồng xử lý và cấu trúc của tính năng Quyết toán hợp đồng.

## 📁 Danh sách tệp tin then chốt (Key Files)
- **Frontend**: `QuanLyDoanKham.Web/src/views/SettlementReport.vue`
- **Controller**: `QuanLyDoanKham.API/Controllers/HealthContractsController.cs` (Line 575+)
- **Service Interface**: `QuanLyDoanKham.API/Services/Settlement/ISettlementService.cs`
- **Service Implementation**: `QuanLyDoanKham.API/Services/Settlement/SettlementService.cs`

## 🔄 Luồng xử lý (Processing Flow)
1. **Request**: UI gọi endpoint `GET /api/HealthContracts/{id}/settlement`.
2. **Controller**: `HealthContractsController` nhận yêu cầu, inject `ISettlementService`.
3. **Logic**: `SettlementService.CalculateSettlementAsync` thực hiện:
    - Lấy thông tin Hợp đồng (`TotalAmount`, `ExpectedQuantity`).
    - Lấy danh sách Nhóm khám (`MedicalGroups`) thuộc hợp đồng.
    - Tính tổng chi phí nhân sự thực tế từ `GroupStaffDetails`.
4. **Response**: Trả về `SettlementResultDto`.

## 📊 Cấu trúc dữ liệu (SettlementResultDto)
- `ContractValue`: Giá trị kế hoạch.
- `ActualValue`: Giá trị thực tế đã khám.
- `ExtraServiceValue`: Giá trị dịch vụ ngoài gói.
- `TotalSettlement`: Tổng quyết toán cuối cùng.

## ⚠️ Lưu ý nghiệp vụ (Business Rules)
- Hệ thống hiện tại đang giả định `ActualValue` bằng `ContractValue` trong phiên bản Lean (cần nâng cấp nếu muốn tính toán chi tiết hơn).
- Chi phí nhân sự được tổng hợp từ chi tiết lương của từng nhân viên trong nhóm.
