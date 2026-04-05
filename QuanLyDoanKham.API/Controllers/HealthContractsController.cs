using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.Security.Claims;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HealthContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // GET: api/HealthContracts
        // ================================================================
        [HttpGet]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<ActionResult<IEnumerable<HealthContractDto>>> GetContracts(
            [FromQuery] string status = null,
            [FromQuery] int? companyId = null)
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
                    ShortName = c.Company.ShortName,
                    CompanyName = c.Company.CompanyName,
                    SigningDate = c.SigningDate,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    UnitPrice = c.UnitPrice,
                    ExpectedQuantity = c.ExpectedQuantity,
                    UnitName = c.UnitName,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status,
                    CurrentApprovalStep = c.CurrentApprovalStep,
                    FilePath = c.FilePath,
                    CreatedByName = c.CreatedByUser != null ? c.CreatedByUser.FullName : null,
                    CreatedAt = c.CreatedAt,
                    Notes = c.Notes,
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

            return Ok(result);
        }

        // GET: api/HealthContracts/{id}
        [HttpGet("{id}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<ActionResult<HealthContractDto>> GetContract(int id)
        {
            var c = await _context.Contracts
                .Include(x => x.Company)
                .Include(x => x.StatusHistories)
                .Include(x => x.ApprovalHistories).ThenInclude(h => h.ApprovedByUser)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Attachments)
                .Include(x => x.MedicalGroups)
                .FirstOrDefaultAsync(x => x.HealthContractId == id);

            if (c == null) return NotFound();

            return Ok(new HealthContractDto
            {
                HealthContractId = c.HealthContractId,
                CompanyId = c.CompanyId,
                ContractCode = c.ContractCode,
                ShortName = c.Company?.ShortName,
                CompanyName = c.Company?.CompanyName,
                SigningDate = c.SigningDate,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UnitPrice = c.UnitPrice,
                ExpectedQuantity = c.ExpectedQuantity,
                UnitName = c.UnitName,
                TotalAmount = c.TotalAmount,
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
            });
        }

        // ================================================================
        // POST: api/HealthContracts — Tạo hợp đồng (Draft)
        // ================================================================
        [HttpPost]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Create")]
        public async Task<ActionResult<HealthContract>> PostContract(HealthContractDto dto)
        {
            if (await _context.Contracts.AnyAsync(c =>
                c.CompanyId == dto.CompanyId && c.SigningDate.Date == dto.SigningDate.Value.Date))
                return BadRequest("Công ty này đã có hợp đồng ký vào ngày này.");

            var username = User.Identity?.Name ?? "system";
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

            // Tự động sinh mã hợp đồng nếu trống
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
                SigningDate = dto.SigningDate.Value,
                StartDate = dto.StartDate.Value,
                EndDate = dto.EndDate.Value,
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
            return Ok(new { message = "Tạo hợp đồng thành công.", contractId = contract.HealthContractId, contractCode });
        }

        // ================================================================
        // PUT: api/HealthContracts/{id}
        // ================================================================
        [HttpPut("{id}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> PutContract(int id, HealthContractDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status == "Locked") return BadRequest("Hợp đồng đã khóa, không thể chỉnh sửa.");
            if (contract.Status is "Approved" or "Active")
                return BadRequest("Hợp đồng đã được duyệt. Chỉ Admin mới có thể chỉnh sửa.");

            var username = User.Identity?.Name ?? "system";

            contract.CompanyId = dto.CompanyId;
            contract.SigningDate = dto.SigningDate.Value;
            contract.StartDate = dto.StartDate.Value;
            contract.EndDate = dto.EndDate.Value;
            contract.UnitPrice = dto.UnitPrice;
            contract.ExpectedQuantity = dto.ExpectedQuantity;
            contract.UnitName = dto.UnitName;
            contract.TotalAmount = dto.UnitPrice * dto.ExpectedQuantity;
            contract.Notes = dto.Notes;
            contract.FilePath = dto.FilePath;
            contract.UpdatedAt = DateTime.Now;
            contract.UpdatedBy = username;

            // Khi sửa lại HĐ bị từ chối → chuyển về Draft
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
            return Ok(new { message = "Cập nhật hợp đồng thành công." });
        }

        // ================================================================
        // POST: api/HealthContracts/{id}/submit — Gửi đi duyệt
        // ================================================================
        [HttpPost("{id}/submit")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Approve")]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status != "Draft") return BadRequest($"Chỉ hợp đồng ở trạng thái Draft mới có thể gửi duyệt (hiện tại: {contract.Status}).");

            var username = User.Identity?.Name ?? "system";
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

            return Ok(new { message = $"Đã gửi hợp đồng đi duyệt. Đang chờ: {firstStep?.StepName}" });
        }

        // ================================================================
        // POST: api/HealthContracts/{id}/approve — Phê duyệt từng bước
        // ================================================================
        [HttpPost("{id}/approve")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Approve")]
        public async Task<IActionResult> ApproveContract(int id, [FromBody] ApprovalActionDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status != "PendingApproval")
                return BadRequest("Hợp đồng không ở trạng thái chờ duyệt.");

            var username = User.Identity?.Name ?? "system";
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

            // Cấm người tạo tự duyệt
            if (contract.CreatedByUserId.HasValue && contract.CreatedByUserId.Value == userId)
                return BadRequest("Người tạo không được tự phê duyệt hợp đồng của chính mình.");

            // Lấy bước hiện tại
            var currentStep = await _context.ContractApprovalSteps
                .Where(s => s.IsActive && s.StepOrder == contract.CurrentApprovalStep)
                .FirstOrDefaultAsync();

            // Lấy bước tiếp theo
            var nextStep = await _context.ContractApprovalSteps
                .Where(s => s.IsActive && s.StepOrder > contract.CurrentApprovalStep)
                .OrderBy(s => s.StepOrder)
                .FirstOrDefaultAsync();

            // Ghi lịch sử duyệt
            _context.ContractApprovalHistories.Add(new ContractApprovalHistory
            {
                HealthContractId = id,
                StepOrder = contract.CurrentApprovalStep,
                StepName = currentStep?.StepName ?? $"Bước {contract.CurrentApprovalStep}",
                Action = "Approved",
                Note = dto.Note,
                ApprovedByUserId = userId,
                ActionDate = DateTime.Now
            });

            if (nextStep != null)
            {
                // Còn bước duyệt tiếp theo
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
                return Ok(new { message = $"Đã phê duyệt bước {currentStep?.StepOrder}. Tiếp theo: {nextStep.StepName}", status = "PendingApproval" });
            }
            else
            {
                // Đã duyệt hết tất cả bước → Approved
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
            return Ok(new { message = "Hợp đồng đã được phê duyệt hoàn toàn!", status = "Approved" });
        }
        }

        // ================================================================
        // POST: api/HealthContracts/{id}/reject — Từ chối
        // ================================================================
        [HttpPost("{id}/reject")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Reject")]
        public async Task<IActionResult> RejectContract(int id, [FromBody] ApprovalActionDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status != "PendingApproval")
                return BadRequest("Chỉ có thể từ chối hợp đồng đang chờ duyệt.");

            var username = User.Identity?.Name ?? "system";
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

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
            return Ok(new { message = "Đã từ chối hợp đồng.", status = "Rejected" });
        }

        // ================================================================
        // PATCH: api/HealthContracts/{id}/status — Admin update trạng thái thủ công
        // ================================================================
        [HttpPatch("{id}/status")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status == dto.Status) return Ok(contract);

            // Bắt buộc có file & signer khi chuyển Signed
            if (dto.Status == "Signed")
            {
                if (string.IsNullOrEmpty(contract.FilePath))
                    return BadRequest("Phải đính kèm file hợp đồng trước khi ký.");
                if (!contract.SignerId.HasValue)
                    return BadRequest("Phải lưu thông tin người ký (SignerId) trước khi ký.");
            }

            if (dto.Status == "Finished")
            {
                var hasUnfinished = await _context.MedicalGroups
                    .AnyAsync(g => g.HealthContractId == id && g.Status != "Finished" && g.Status != "Locked");
                if (hasUnfinished)
                    return BadRequest("Còn đoàn khám chưa hoàn thành trong hợp đồng này.");
            }

            var username = User.Identity?.Name ?? "system";
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
            return Ok(new { message = "Đã cập nhật trạng thái.", status = contract.Status });
        }

        // ================================================================
        // POST: api/HealthContracts/{id}/attachments — Upload file đính kèm
        // ================================================================
        [HttpPost("{id}/attachments")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Upload")]
        public async Task<IActionResult> UploadAttachment(int id, [FromForm] IFormFile file)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (file == null || file.Length == 0) return BadRequest("Tệp không hợp lệ.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowedExt = new[] { ".pdf", ".doc", ".docx", ".jpg", ".png", ".xlsx" };
            if (!allowedExt.Contains(ext))
                return BadRequest("Định dạng file không hỗ trợ. Chỉ nhận: PDF, Word, Excel, ảnh.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativePath = $"uploads/contracts/{fileName}";
            var attachment = new ContractAttachment
            {
                HealthContractId = id,
                FileName = file.FileName,
                FilePath = relativePath,
                FileType = ext.TrimStart('.').ToUpper(),
                UploadedAt = DateTime.Now,
                UploadedBy = User.Identity?.Name ?? "system"
            };

            // Cập nhật FilePath chính nếu chưa có
            if (string.IsNullOrEmpty(contract.FilePath))
                contract.FilePath = relativePath;

            _context.ContractAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Upload thành công.", path = relativePath, fileName = file.FileName });
        }

        // ================================================================
        // DELETE: api/HealthContracts/{id}
        // ================================================================
        [HttpDelete("{id}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status == "Locked") return BadRequest("Hợp đồng đang khóa, mở khóa trước khi xóa.");

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa hợp đồng thành công." });
        }

        // PUT: api/HealthContracts/{id}/lock
        [HttpPut("{id}/lock")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> LockContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status != "Locked")
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id, OldStatus = contract.Status, NewStatus = "Locked",
                    ChangedBy = User.Identity?.Name ?? "system", ChangedAt = DateTime.Now, Note = "Khóa hợp đồng"
                });
                contract.Status = "Locked";
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Hợp đồng đã được khóa." });
        }

        // PUT: api/HealthContracts/{id}/unlock
        [HttpPut("{id}/unlock")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> UnlockContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id, OldStatus = contract.Status, NewStatus = "Approved",
                ChangedBy = User.Identity?.Name ?? "system", ChangedAt = DateTime.Now, Note = "Mở khóa hợp đồng"
            });
            contract.Status = "Approved";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã mở khóa hợp đồng." });
        }

        // GET: api/HealthContracts/approval-steps
        [HttpGet("approval-steps")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<IActionResult> GetApprovalSteps()
        {
            var steps = await _context.ContractApprovalSteps
                .Where(s => s.IsActive)
                .OrderBy(s => s.StepOrder)
                .ToListAsync();
            return Ok(steps);
        }
    }
}
