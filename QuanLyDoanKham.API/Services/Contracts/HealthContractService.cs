using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

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

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);
            if (companyId.HasValue)
                query = query.Where(c => c.CompanyId == companyId.Value);

            var result = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new HealthContractDto
                {
                    HealthContractId = c.HealthContractId,
                    CompanyId = c.CompanyId,
                    ContractCode = c.ContractCode,
                    ContractName = c.ContractName,
                    ShortName = c.Company != null ? c.Company.ShortName : null,
                    CompanyName = c.Company != null ? c.Company.CompanyName : null,
                    SigningDate = c.SigningDate,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    UnitPrice = c.UnitPrice,
                    ExpectedQuantity = c.ExpectedQuantity,
                    UnitName = c.UnitName,
                    TotalAmount = c.TotalAmount,
                    FinalSettlementValue = c.FinalSettlementValue,
                    Status = c.Status,
                    CurrentApprovalStep = c.CurrentApprovalStep,
                    FilePath = c.FilePath,
                    CreatedByName = c.CreatedByUser != null ? c.CreatedByUser.FullName : null,
                    CreatedAt = c.CreatedAt,
                    Notes = c.Notes,
                    TotalGroups = c.MedicalGroups.Count,
                    StatusHistories = c.StatusHistories
                        .OrderByDescending(h => h.ChangedAt)
                        .Select(h => new ContractStatusHistoryDto
                        {
                            Id = h.Id,
                            OldStatus = h.OldStatus,
                            NewStatus = h.NewStatus,
                            Note = h.Note,
                            ChangedAt = h.ChangedAt,
                            ChangedBy = h.ChangedBy
                        }).ToList(),
                    ApprovalHistories = c.ApprovalHistories
                        .OrderBy(h => h.StepOrder)
                        .Select(h => new ContractApprovalHistoryDto
                        {
                            Id = h.Id,
                            StepOrder = h.StepOrder,
                            StepName = h.StepName,
                            Action = h.Action,
                            Note = h.Note,
                            ApprovedByName = h.ApprovedByUser != null ? h.ApprovedByUser.FullName : null,
                            ActionDate = h.ActionDate
                        }).ToList()
                })
                .ToListAsync();

            return result;
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
                Status = c.Status,
                CurrentApprovalStep = c.CurrentApprovalStep,
                FilePath = c.FilePath,
                CreatedByName = c.CreatedByUser?.FullName,
                CreatedAt = c.CreatedAt,
                Notes = c.Notes,
                TotalGroups = c.MedicalGroups.Count,
                StatusHistories = c.StatusHistories
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => new ContractStatusHistoryDto
                    {
                        Id = h.Id, OldStatus = h.OldStatus, NewStatus = h.NewStatus,
                        Note = h.Note, ChangedAt = h.ChangedAt, ChangedBy = h.ChangedBy
                    }).ToList(),
                ApprovalHistories = c.ApprovalHistories
                    .OrderBy(h => h.StepOrder)
                    .Select(h => new ContractApprovalHistoryDto
                    {
                        Id = h.Id, StepOrder = h.StepOrder, StepName = h.StepName,
                        Action = h.Action, Note = h.Note,
                        ApprovedByName = h.ApprovedByUser?.FullName,
                        ActionDate = h.ActionDate
                    }).ToList(),
                Attachments = c.Attachments
                    .Select(a => new ContractAttachmentDto
                    {
                        Id = a.Id, FileName = a.FileName,
                        FilePath = a.FilePath, FileType = a.FileType,
                        UploadedAt = a.UploadedAt, UploadedBy = a.UploadedBy
                    }).ToList()
            };
        }

        public async Task<(HealthContract? Contract, string? ErrorMessage)> CreateContractAsync(HealthContractDto dto, string username, int? userId)
        {
            if (await _context.Contracts.AnyAsync(c =>
                c.CompanyId == dto.CompanyId && c.SigningDate.Date == dto.SigningDate.Date))
                return (null, "Công ty này đã có hợp đồng ký vào ngày này.");

            var contractCode = dto.ContractCode;
            if (string.IsNullOrEmpty(contractCode))
            {
                var count = await _context.Contracts.CountAsync();
                contractCode = $"HD{DateTime.Now.Year}{(count + 1):D4}";
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
                Status = "Draft",
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
            if (contract.Status == "Locked") return (false, "Hợp đồng đã khóa, không thể chỉnh sửa.");
            if (contract.Status is "Approved" or "Active")
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

            if (contract.Status == "Rejected")
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = contract.Status,
                    NewStatus = "Draft",
                    ChangedBy = username,
                    ChangedAt = DateTime.Now,
                    Note = "Chỉnh sửa lại sau khi bị từ chối"
                });
                contract.Status = "Draft";
                contract.CurrentApprovalStep = 0;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, string? StatusName)> SubmitForApprovalAsync(int id, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found", null);
            if (contract.Status != "Draft") return (false, $"Chỉ hợp đồng ở trạng thái Draft mới có thể gửi duyệt (hiện tại: {contract.Status}).", null);

            var firstStep = await _context.ContractApprovalSteps
                .Where(s => s.IsActive).OrderBy(s => s.StepOrder).FirstOrDefaultAsync();

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id,
                OldStatus = "Draft",
                NewStatus = "PendingApproval",
                ChangedBy = username,
                ChangedAt = DateTime.Now,
                Note = $"Gửi phê duyệt - Bước {firstStep?.StepOrder}: {firstStep?.StepName}"
            });

            contract.Status = "PendingApproval";
            contract.CurrentApprovalStep = firstStep?.StepOrder ?? 1;
            await _context.SaveChangesAsync();

            return (true, null, firstStep?.StepName);
        }

        public async Task<(bool Success, string? ErrorMessage, string? NewStatus)> ApproveContractAsync(int id, ApprovalActionDto dto, string username, int userId)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found", null);
            if (contract.Status != "PendingApproval") return (false, "Hợp đồng không ở trạng thái chờ duyệt.", null);

            if (contract.CreatedByUserId.HasValue && contract.CreatedByUserId.Value == userId)
                return (false, "Người tạo không được tự phê duyệt hợp đồng của chính mình.", null);

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
                    OldStatus = "PendingApproval",
                    NewStatus = "PendingApproval",
                    ChangedBy = username,
                    ChangedAt = DateTime.Now,
                    Note = $"Đã duyệt bước {currentStep?.StepOrder}. Đang chờ: {nextStep.StepName}"
                });
                await _context.SaveChangesAsync();
                return (true, null, $"PendingApproval_Next_{nextStep.StepName}_{currentStep?.StepOrder}");
            }
            else
            {
                contract.Status = "Approved";
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = "PendingApproval",
                    NewStatus = "Approved",
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
            if (contract.Status != "PendingApproval") return (false, "Chỉ có thể từ chối hợp đồng đang chờ duyệt.");

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
                OldStatus = contract.Status,
                NewStatus = "Rejected",
                ChangedBy = username,
                ChangedAt = DateTime.Now,
                Note = dto.Note ?? "Từ chối hợp đồng"
            });

            contract.Status = "Rejected";
            contract.CurrentApprovalStep = 0;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(int id, StatusUpdateDto dto, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status == dto.Status) return (true, null);

            if (dto.Status == "Signed")
            {
                if (string.IsNullOrEmpty(contract.FilePath))
                    return (false, "Phải đính kèm file hợp đồng trước khi ký.");
                if (!contract.SignerId.HasValue)
                    return (false, "Phải lưu thông tin người ký (SignerId) trước khi ký.");
            }

            if (dto.Status == "Finished")
            {
                var unfinishedGroups = await _context.MedicalGroups
                    .Where(g => g.HealthContractId == id && g.Status != "Finished" && g.Status != "Locked")
                    // Chỉ chặn nếu đoàn khám thực sự có dữ liệu (Bệnh nhân hoặc Nhân viên đã phân công)
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

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id,
                OldStatus = contract.Status,
                NewStatus = dto.Status,
                ChangedBy = username,
                ChangedAt = DateTime.Now,
                Note = dto.Note ?? $"Admin chuyển trạng thái sang {dto.Status}"
            });

            contract.Status = dto.Status;
            contract.UpdatedAt = DateTime.Now;
            contract.UpdatedBy = username;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteContractAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status == "Locked") return (false, "Hợp đồng đang khóa, mở khóa trước khi xóa.");

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> LockContractAsync(int id, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status != "Locked")
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id, OldStatus = contract.Status, NewStatus = "Locked",
                    ChangedBy = username, ChangedAt = DateTime.Now, Note = "Khóa hợp đồng"
                });
                contract.Status = "Locked";
                await _context.SaveChangesAsync();
            }
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UnlockContractAsync(int id, string username)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return (false, "Not Found");
            if (contract.Status != "Approved" && contract.Status != "Locked") return (false, "Khởi động lại"); // Custom edge

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id, OldStatus = contract.Status, NewStatus = "Approved",
                ChangedBy = username, ChangedAt = DateTime.Now, Note = "Mở khóa hợp đồng"
            });
            contract.Status = "Approved";
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
