-- ThĂªm cĂ¡c bÆ°á»›c phĂª duyá»‡t máº·c Ä‘á»‹nh cho Há»£p Ä‘á»“ng
INSERT INTO ContractApprovalSteps (StepOrder, StepName, RequiredPermission, IsActive)
VALUES 
(1, N'TrÆ°á»Ÿng phĂ²ng kinh doanh duyá»‡t', 'HopDong.Approve', 1),
(2, N'Káº¿ toĂ¡n trÆ°á»Ÿng kiá»ƒm tra', 'HopDong.Approve', 1),
(3, N'GiĂ¡m Ä‘á»‘c phĂª duyá»‡t cuá»‘i', 'HopDong.Approve', 1);

-- CĂ´ng bá»‘ káº¿t quáº£
SELECT * FROM ContractApprovalSteps;
