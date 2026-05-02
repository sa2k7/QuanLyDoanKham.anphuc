-- Thêm các bước phê duyệt mặc định cho Hợp đồng
INSERT INTO ContractApprovalSteps (StepOrder, StepName, RequiredPermission, IsActive)
VALUES 
(1, N'Trưởng phòng kinh doanh duyệt', 'HopDong.Approve', 1),
(2, N'Kế toán trưởng kiểm tra', 'HopDong.Approve', 1),
(3, N'Giám đốc phê duyệt cuối', 'HopDong.Approve', 1);

-- Công bố kết quả
SELECT * FROM ContractApprovalSteps;
