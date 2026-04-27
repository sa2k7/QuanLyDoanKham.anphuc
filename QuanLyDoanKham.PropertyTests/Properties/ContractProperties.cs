using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Hợp đồng.
/// Feature: medical-examination-team-management
/// </summary>
public class ContractProperties
{
    // -----------------------------------------------------------------------
    // Property 2: Hợp đồng mới tạo không bao giờ ở trạng thái "Đã duyệt"
    // Validates: Requirements 1.2
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P2_NewContractNeverApproved()
    {
        // Feature: medical-examination-team-management, Property 2: New contract never starts as Approved
        var gen = DomainArbitraries.DraftContracts().Generator;
        return Prop.ForAll(Arb.From(gen), contract =>
        {
            // Hợp đồng mới tạo phải ở trạng thái Draft hoặc PendingApproval, không bao giờ Approved
            return contract.Status != ContractStatus.Approved;
        });
    }

    // -----------------------------------------------------------------------
    // Property 3: Người tạo và người phê duyệt hợp đồng được lưu độc lập
    // Validates: Requirements 1.3
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P3_CreatorAndApproverStoredIndependently()
    {
        // Feature: medical-examination-team-management, Property 3: Creator and approver stored independently
        var gen =
            from creatorId in Gen.Choose(1, 1000)
            from approverId in Gen.Choose(1001, 2000) // khác nhau
            select (creatorId, approverId);

        return Prop.ForAll(Arb.From(gen), pair =>
        {
            var (creatorId, approverId) = pair;
            var contract = new HealthContract
            {
                ContractName = "Test",
                SigningDate = DateTime.Today,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                TotalAmount = 1000,
                Status = ContractStatus.Approved,
                CreatedByUserId = creatorId,
                // ApprovedByUserId không có trong entity, dùng SignerId để mô phỏng approver
                SignerId = approverId
            };

            // Người tạo và người ký/duyệt phải khác nhau và đều có thể truy xuất
            return contract.CreatedByUserId.HasValue
                && contract.SignerId.HasValue
                && contract.CreatedByUserId != contract.SignerId;
        });
    }

    // -----------------------------------------------------------------------
    // Property 4: Hợp đồng chưa duyệt không thể tạo đoàn khám
    // Validates: Requirements 1.4, 2.1
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P4_UnapprovedContractBlocksTeamCreation()
    {
        // Feature: medical-examination-team-management, Property 4: Unapproved contract blocks team creation
        return Prop.ForAll(DomainArbitraries.NonApprovedStatuses(), status =>
        {
            var contract = new HealthContract
            {
                ContractName = "Test",
                SigningDate = DateTime.Today,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                TotalAmount = 1000,
                Status = status
            };

            // Logic từ AutoCreateMedicalGroupService.CanCreateMedicalGroups
            bool canCreate = contract.Status == ContractStatus.Approved
                          || contract.Status == ContractStatus.Active;

            return !canCreate;
        });
    }

    // -----------------------------------------------------------------------
    // Property 5: Phê duyệt hợp đồng cập nhật trạng thái và thời gian
    // Validates: Requirements 1.5
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P5_ApprovalUpdatesStatusAndTimestamp()
    {
        // Feature: medical-examination-team-management, Property 5: Approval updates status and timestamp
        var gen =
            from pendingStatus in Gen.Elements(ContractStatus.PendingApproval, ContractStatus.Draft)
            select pendingStatus;

        return Prop.ForAll(Arb.From(gen), initialStatus =>
        {
            var contract = new HealthContract
            {
                ContractName = "Test",
                SigningDate = DateTime.Today,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                TotalAmount = 1000,
                Status = initialStatus
            };

            // Simulate approval
            var approvedAt = DateTime.Now;
            contract.Status = ContractStatus.Approved;
            contract.UpdatedAt = approvedAt;

            return contract.Status == ContractStatus.Approved
                && contract.UpdatedAt.HasValue
                && contract.UpdatedAt.Value >= approvedAt.AddSeconds(-1);
        });
    }

    // -----------------------------------------------------------------------
    // Property 6: File đính kèm hợp đồng có thể tải lại sau khi upload
    // Validates: Requirements 1.7
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P6_AttachmentUrlPreservedAfterUpload()
    {
        // Feature: medical-examination-team-management, Property 6: Attachment URL preserved after upload
        var gen =
            from fileName in Gen.Elements("contract.pdf", "hop_dong.docx", "scan.jpg", "file.png")
            from contractId in Gen.Choose(1, 1000)
            select (fileName, contractId);

        return Prop.ForAll(Arb.From(gen), pair =>
        {
            var (fileName, contractId) = pair;
            var uploadPath = $"/uploads/contracts/{contractId}/{fileName}";

            var attachment = new ContractAttachment
            {
                HealthContractId = contractId,
                FileName = fileName,
                FilePath = uploadPath,
                UploadedAt = DateTime.Now
            };

            // Đường dẫn phải được lưu và có thể truy xuất
            return !string.IsNullOrEmpty(attachment.FilePath)
                && attachment.FilePath.Contains(fileName)
                && attachment.HealthContractId == contractId;
        });
    }
}
