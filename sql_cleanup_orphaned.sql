-- ==============================================================================
-- SCRIPT DỌN DẸP DATABASE (CLEANUP ORPHANED TABLES)
-- Dự án: Quản Lý Đoàn Khám (2026)
-- 
-- MỤC TIÊU: Xóa các bảng cũ không còn được code sử dụng để làm sạch Database
--           và chuẩn bị cho việc đồng bộ hóa (Migration Update).
-- ==============================================================================

-- [BƯỚC 1] XÓA CÁC BẢNG ORPHANED (KHÔNG CÓ TRONG CODE HIỆN TẠI)
-- Lưu ý: Hãy đảm bảo bạn đã Backup dữ liệu nếu các bảng này chứa thông tin cũ quan trọng.

IF OBJECT_ID('PayrollRecords', 'U') IS NOT NULL DROP TABLE [PayrollRecords];
IF OBJECT_ID('SupplyInventoryDetails', 'U') IS NOT NULL DROP TABLE [SupplyInventoryDetails];
IF OBJECT_ID('SupplyInventoryVouchers', 'U') IS NOT NULL DROP TABLE [SupplyInventoryVouchers];
IF OBJECT_ID('Supplies', 'U') IS NOT NULL DROP TABLE [Supplies];
IF OBJECT_ID('SupplyTransactions', 'U') IS NOT NULL DROP TABLE [SupplyTransactions];
IF OBJECT_ID('GroupSupplyDetails', 'U') IS NOT NULL DROP TABLE [GroupSupplyDetails];

-- Các bảng tạm hoặc bảng cũ từ module OMS (đã xóa)
IF OBJECT_ID('OmsTasks', 'U') IS NOT NULL DROP TABLE [OmsTasks];
IF OBJECT_ID('OmsQueues', 'U') IS NOT NULL DROP TABLE [OmsQueues];

GO

-- [BƯỚC 2] XÓA CÁC BẢNG ĐÃ ĐƯỢC REFACTOR SANG MEDICALRECORD (NẾU CẦN)
-- (Chỉ thực hiện nếu bạn muốn làm mới hoàn toàn dữ liệu lâm sàng)
-- DROP TABLE IF EXISTS [Departments];
-- DROP TABLE IF EXISTS [WorkRules];
-- DROP TABLE IF EXISTS [MedicalGroupPatients];

PRINT 'Cleanup complete. Now run "dotnet ef database update" to sync the modern schema.';
