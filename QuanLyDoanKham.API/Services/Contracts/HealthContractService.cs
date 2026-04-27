using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Services.Contracts
{
    public class HealthContractService : IHealthContractService
    {
        private readonly ApplicationDbContext _context;

        public HealthContractService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HealthContractDto>> GetAllContractsAsync(string? status, int? companyId)
        {
            var query = _context.Contracts
                .Include(c => c.Company)
                .Include(c => c.StatusHistories)
                .Include(c => c.ApprovalHistories).ThenInclude(h => h.ApprovedByUser)
                .Include(c => c.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContractStatus>(status, true, out var contractStatus))
                query = query.Where(c => c.Status == contractStatus);
            if (companyId.HasValue)
                query = query.Where(c => c.CompanyId == companyId.Value);

            var result = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return result.Select(MapToDto);
        }

        public async Task<HealthContractDto?> GetContractByIdAsync(int id)
        {
            var c = await _context.Contracts
                .Include(x => x.Company)
                .Include(x => x.StatusHistories)
                .Include(x => x.ApprovalHistories).ThenInclude(h => h.ApprovedByUser)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Attachments)
                .Include(x => x.MedicalGroups)
                .FirstOrDefaultAsync(x => x.HealthContractId == id);

            if (c == null) return null;

            return MapToDto(c);
        }

        public async Task<(HealthContract? Contract, string? ErrorMessage)> CreateContractAsync(HealthContractDto dto, string username, int? userId)
        {
            if (await _context.Contracts.AnyAsync(c =>
                c.CompanyId == dto.CompanyId && c.SigningDate.Date == dto.SigningDate.Date))
                return (null, "Công ty này đã có hợp đồng ký vào ngày này.");

            var contractCode = dto.ContractCode;
            if (string.IsNullOrEmpty(contractCode))
            {
                contractCode = await GenerateContractCodeAsync();
            }

            var contract = new HealthContract
            {
                CompanyId = dto.CompanyId,
                ContractCode = contractCode,
                ContractName = dto.ContractName,
                SigningDate = dto.SigningDate,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UnitPrice = dto.UnitPrice,
                ExpectedQuantity = dto.ExpectedQuantity,
                UnitName = string.IsNullOrEmpty(dto.UnitName) ? "Người" : dto.UnitName,
                TotalAmount = dto.UnitPrice * dto.ExpectedQuantity,
                Status = ContractStatus.Draft,
                FilePath = dto.FilePath,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now,
                CurrentApprovalStep = 0
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return (contract, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateContractAsync(int id, HealthContractDto dto, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status == ContractStatus.Locked) return (false, "Hợp đồng đã khóa, không thể chỉnh sửa.");
            if (contract.Status is ContractStatus.Approved or ContractStatus.Active)
                return (false, "Hợp đồng đã được duyệt. Chỉ Admin mới có thể chỉnh sửa.");

            contract.CompanyId = dto.CompanyId;
            contract.ContractName = dto.ContractName;
            contract.SigningDate = dto.SigningDate;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.UnitPrice = dto.UnitPrice;
            contract.ExpectedQuantity = dto.ExpectedQuantity;
            contract.UnitName = dto.UnitName;
            contract.TotalAmount = dto.UnitPrice * dto.ExpectedQuantity;
            contract.Notes = dto.Notes;
            contract.FilePath = dto.FilePath;
            contract.UpdatedAt = DateTime.Now;
            contract.UpdatedBy = username;

            if (contract.Status == ContractStatus.Rejected)
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = contract.Status.ToString(),
                    NewStatus = ContractStatus.Draft.ToString(),
                    ChangedBy = username,
                    ChangedAt = DateTime.Now,
                    Note = "Chỉnh sửa lại sau khi bị từ chối"
                });
                contract.Status = ContractStatus.Draft;
                contract.CurrentApprovalStep = 0;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, string? StatusName)> SubmitForApprovalAsync(int id, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found", null);
            if (contract.Status != ContractStatus.Draft) return (false, $"Chỉ hợp đồng ở trạng thái Draft mới có thể gửi duyệt (hiện tại: {contract.Status}).", null);

            var firstStep = await _context.ContractApprovalSteps
                .Where(s => s.IsActive).OrderBy(s => s.StepOrder).FirstOrDefaultAsync();

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id,
                OldStatus = ContractStatus.Draft.ToString(),
                NewStatus = ContractStatus.PendingApproval.ToString(),
                ChangedBy = username,
                ChangedAt = DateTime.Now,
                Note = $"Gửi phê duyệt - Bước {firstStep?.StepOrder}: {firstStep?.StepName}"
            });

            contract.Status = ContractStatus.PendingApproval;
            contract.CurrentApprovalStep = firstStep?.StepOrder ?? 1;
            await _context.SaveChangesAsync();

            return (true, null, firstStep?.StepName);
        }

        public async Task<(bool Success, string? ErrorMessage, string? NewStatus)> ApproveContractAsync(int id, ApprovalActionDto dto, string username, int userId)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found", null);
            if (contract.Status != ContractStatus.PendingApproval) return (false, "Hợp đồng không ở trạng thái chờ duyệt.", null);

            var user = await _context.Users.FindAsync(userId);
            bool isAdmin = user?.RoleId == 1;

            if (!isAdmin && contract.CreatedByUserId.HasValue && contract.CreatedByUserId.Value == userId)
                return (false, "Người tạo không được tự phê duyệt hợp đồng của chính mình. (Chỉ Admin mới có quyền này)", null);

            var currentStep = await _context.ContractApprovalSteps
                .Where(s => s.IsActive && s.StepOrder == contract.CurrentApprovalStep)
                .FirstOrDefaultAsync();

            var nextStep = await _context.ContractApprovalSteps
                .Where(s => s.IsActive && s.StepOrder > contract.CurrentApprovalStep)
                .OrderBy(s => s.StepOrder)
                .FirstOrDefaultAsync();

            _context.ContractApprovalHistories.Add(new ContractApprovalHistory
            {
                HealthContractId = id,
                StepOrder = contract.CurrentApprovalStep,
                StepName = currentStep?.StepName ?? $"Bước {contract.CurrentApprovalStep}",
                Action = "Approved",
                Note = dto.Note ?? "",
                ApprovedByUserId = userId,
                ActionDate = DateTime.Now
            });

            if (nextStep != null)
            {
                contract.CurrentApprovalStep = nextStep.StepOrder;
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = ContractStatus.PendingApproval.ToString(),
                    NewStatus = ContractStatus.PendingApproval.ToString(),
                    ChangedBy = username,
                    ChangedAt = DateTime.Now,
                    Note = $"Đã duyệt bước {currentStep?.StepOrder}. Đang chờ: {nextStep.StepName}"
                });
                await _context.SaveChangesAsync();
                return (true, null, $"PendingApproval_Next_{nextStep.StepName}_{currentStep?.StepOrder}");
            }
            else
            {
                contract.Status = ContractStatus.Approved;
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = ContractStatus.PendingApproval.ToString(),
                    NewStatus = ContractStatus.Approved.ToString(),
                    ChangedBy = username,
                    ChangedAt = DateTime.Now,
                    Note = dto.Note ?? "Phê duyệt hợp đồng hoàn tất"
                });
                await _context.SaveChangesAsync();
                return (true, null, "Approved");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectContractAsync(int id, ApprovalActionDto dto, string username, int userId)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status != ContractStatus.PendingApproval) return (false, "Chỉ có thể từ chối hợp đồng đang chờ duyệt.");

            _context.ContractApprovalHistories.Add(new ContractApprovalHistory
            {
                HealthContractId = id,
                StepOrder = contract.CurrentApprovalStep,
                StepName = $"Bước {contract.CurrentApprovalStep}",
                Action = "Rejected",
                Note = dto.Note ?? "Từ chối hợp đồng",
                ApprovedByUserId = userId,
                ActionDate = DateTime.Now
            });

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id,
                OldStatus = contract.Status.ToString(),
                NewStatus = ContractStatus.Rejected.ToString(),
                ChangedBy = username,
                ChangedAt = DateTime.Now,
                Note = dto.Note ?? "Từ chối hợp đồng"
            });

            contract.Status = ContractStatus.Rejected;
            contract.CurrentApprovalStep = 0;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(int id, StatusUpdateDto dto, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (Enum.TryParse<ContractStatus>(dto.Status, true, out var dtoStatus) && contract.Status == dtoStatus) return (true, null);

            if (dto.Status == "Signed")
            {
                if (string.IsNullOrEmpty(contract.FilePath))
                    return (false, "Phải đính kèm file hợp đồng trước khi ký.");
                if (!contract.SignerId.HasValue)
                    return (false, "Phải lưu thông tin người ký (SignerId) trước khi ký.");
            }

            if (dto.Status == "Finished")
            {
                // Truy vấn người dùng từ cơ sở dữ liệu để kiểm tra quyền Admin (RoleId = 1)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                bool isAdmin = user?.RoleId == 1;

                if (!isAdmin)
                {
                    // Chỉ cộng tác viên hoặc quản lý thông thường mới bị chặn nếu có đoàn khám chưa hoàn thành
                    var unfinishedGroups = await _context.MedicalGroups
                        .Where(g => g.HealthContractId == id && g.Status != "Finished" && g.Status != "Locked")
                        .Where(g => _context.MedicalRecords.Any(mr => mr.GroupId == g.GroupId) || 
                                    _context.GroupStaffDetails.Any(sd => sd.GroupId == g.GroupId))
                        .Select(g => g.GroupName)
                        .ToListAsync();

                    if (unfinishedGroups.Any())
                    {
                        var groupNames = string.Join(", ", unfinishedGroups);
                        return (false, $"Không thể kết thúc hợp đồng vì còn {unfinishedGroups.Count} đoàn khám chưa hoàn thành: {groupNames}");
                    }
                }
            }

            if (!Enum.TryParse<ContractStatus>(dto.Status, true, out var newStatus))
                return (false, $"Trạng thái '{dto.Status}' không hợp lệ.");

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id,
                OldStatus = contract.Status.ToString(),
                NewStatus = dto.Status,
                ChangedBy = username,
                ChangedAt = DateTime.Now,
                Note = dto.Note ?? $"Admin chuyển trạng thái sang {dto.Status}"
            });

            contract.Status = newStatus;
            contract.UpdatedAt = DateTime.Now;
            contract.UpdatedBy = username;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteContractAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status == ContractStatus.Locked) return (false, "Hợp đồng đang khóa, mở khóa trước khi xóa.");

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> LockContractAsync(int id, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status != ContractStatus.Locked)
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id, OldStatus = contract.Status.ToString(), NewStatus = ContractStatus.Locked.ToString(),
                    ChangedBy = username, ChangedAt = DateTime.Now, Note = "Khóa hợp đồng"
                });
                contract.Status = ContractStatus.Locked;
                await _context.SaveChangesAsync();
            }
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UnlockContractAsync(int id, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status != ContractStatus.Approved && contract.Status != ContractStatus.Locked) return (false, "Khởi động lại"); // Custom edge

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id, OldStatus = contract.Status.ToString(), NewStatus = ContractStatus.Approved.ToString(),
                ChangedBy = username, ChangedAt = DateTime.Now, Note = "Mở khóa hợp đồng"
            });
            contract.Status = ContractStatus.Approved;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        private HealthContractDto MapToDto(HealthContract c)
        {
            return new HealthContractDto
            {
                HealthContractId = c.HealthContractId,
                CompanyId = c.CompanyId,
                ContractCode = c.ContractCode,
                ContractName = c.ContractName,
                ShortName = c.Company?.ShortName,
                CompanyName = c.Company?.CompanyName,
                SigningDate = c.SigningDate,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UnitPrice = c.UnitPrice,
                ExpectedQuantity = c.ExpectedQuantity,
                UnitName = c.UnitName,
                TotalAmount = c.TotalAmount,
                FinalSettlementValue = c.FinalSettlementValue,
                Status = c.Status.ToString(),
                CurrentApprovalStep = c.CurrentApprovalStep,
                FilePath = c.FilePath,
                CreatedByName = c.CreatedByUser?.FullName,
                CreatedAt = c.CreatedAt,
                Notes = c.Notes,
                TotalGroups = c.MedicalGroups?.Count ?? 0,
                StatusHistories = c.StatusHistories?
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => new ContractStatusHistoryDto
                    {
                        Id = h.Id, OldStatus = h.OldStatus, NewStatus = h.NewStatus,
                        Note = h.Note, ChangedAt = h.ChangedAt, ChangedBy = h.ChangedBy
                    }).ToList() ?? new(),
                ApprovalHistories = c.ApprovalHistories?
                    .OrderBy(h => h.StepOrder)
                    .Select(h => new ContractApprovalHistoryDto
                    {
                        Id = h.Id, StepOrder = h.StepOrder, StepName = h.StepName,
                        Action = h.Action, Note = h.Note,
                        ApprovedByName = h.ApprovedByUser?.FullName,
                        ActionDate = h.ActionDate
                    }).ToList() ?? new(),
                Attachments = c.Attachments?
                    .Select(a => new ContractAttachmentDto
                    {
                        Id = a.Id, FileName = a.FileName,
                        FilePath = a.FilePath, FileType = a.FileType,
                        UploadedAt = a.UploadedAt, UploadedBy = a.UploadedBy
                    }).ToList() ?? new()
            };
        }

        private async Task<string> GenerateContractCodeAsync()
        {
            var count = await _context.Contracts.CountAsync();
            return $"HD{DateTime.Now.Year}{(count + 1):D4}";
        }
    }
}
