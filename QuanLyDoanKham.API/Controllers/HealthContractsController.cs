using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services.Contracts;
using QuanLyDoanKham.API.Services.Settlement;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/Contracts")]
    [ApiController]
    [Authorize] // Ensure authenticated; individual actions have fine-grained [AuthorizePermission]
    public class HealthContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly QuanLyDoanKham.API.Services.Settlement.ISettlementService _settlementService;
        private readonly QuanLyDoanKham.API.Services.Settlement.ICostCalculationService _costCalculationService;
        private readonly IHealthContractService _contractService;
        private readonly IFinancialCalculatorEngine _financialEngine;

        public HealthContractsController(
            ApplicationDbContext context,
            QuanLyDoanKham.API.Services.Settlement.ISettlementService settlementService,
            QuanLyDoanKham.API.Services.Settlement.ICostCalculationService costCalculationService,
            IHealthContractService contractService,
            IFinancialCalculatorEngine financialEngine)
        {
            _context = context;
            _settlementService = settlementService;
            _costCalculationService = costCalculationService;
            _contractService = contractService;
            _financialEngine = financialEngine;
        }

        [HttpGet]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<ActionResult<IEnumerable<HealthContractDto>>> GetContracts([FromQuery] string? status = null, [FromQuery] int? companyId = null)
        {
            var result = await _contractService.GetAllContractsAsync(status, companyId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<ActionResult<HealthContractDto>> GetContract(int id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null) return NotFound();
            return Ok(contract);
        }

        [HttpPost]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Create")]
        public async Task<ActionResult<HealthContract>> PostContract(HealthContractDto dto)
        {
            var username = User.Identity?.Name ?? "system";
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

            var (contract, error) = await _contractService.CreateContractAsync(dto, username, userId);
            if (error != null) return BadRequest(error);

            return Ok(new { message = "Tạo hợp đồng thành công.", contractId = contract?.HealthContractId, contractCode = contract?.ContractCode });
        }

        [HttpPut("{id}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> PutContract(int id, HealthContractDto dto)
        {
            var username = User.Identity?.Name ?? "system";
            var (success, error) = await _contractService.UpdateContractAsync(id, dto, username);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }
            return Ok(new { message = "Cập nhật hợp đồng thành công." });
        }

        [HttpPost("{id}/submit")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Approve")]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            var username = User.Identity?.Name ?? "system";
            var (success, error, stepName) = await _contractService.SubmitForApprovalAsync(id, username);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }
            return Ok(new { message = $"Đã gửi hợp đồng đi duyệt. Đang chờ: {stepName}" });
        }

        [HttpPost("{id}/approve")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Approve")]
        public async Task<IActionResult> ApproveContract(int id, [FromBody] ApprovalActionDto dto)
        {
            var username = User.Identity?.Name ?? "system";
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

            var (success, error, newStatus) = await _contractService.ApproveContractAsync(id, dto, username, userId);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }

            if (newStatus == "Approved")
                return Ok(new { message = "Hợp đồng đã được phê duyệt hoàn toàn!", status = "Approved" });
            
            var parts = newStatus?.Split('_') ?? Array.Empty<string>();
            var step = parts.Length > 2 ? parts[2] : "";
            var next = parts.Length > 1 ? parts[1] : "";
            return Ok(new { message = $"Đã phê duyệt bước {step}. Tiếp theo: {next}", status = "PendingApproval" });
        }

        [HttpPost("{id}/reject")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Reject")]
        public async Task<IActionResult> RejectContract(int id, [FromBody] ApprovalActionDto dto)
        {
            var username = User.Identity?.Name ?? "system";
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

            var (success, error) = await _contractService.RejectContractAsync(id, dto, username, userId);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }
            return Ok(new { message = "Đã từ chối hợp đồng.", status = "Rejected" });
        }

        [HttpPatch("{id}/status")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var username = User.Identity?.Name ?? "system";
            var (success, error) = await _contractService.UpdateStatusAsync(id, dto, username);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }
            return Ok(new { message = "Đã cập nhật trạng thái.", status = dto.Status });
        }

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

            if (string.IsNullOrEmpty(contract.FilePath))
                contract.FilePath = relativePath;

            _context.ContractAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Upload thành công.", path = relativePath, fileName = file.FileName });
        }

        [HttpGet("{id}/attachments")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<IActionResult> GetAttachments(int id)
        {
            var attachments = await _context.ContractAttachments
                .Where(a => a.HealthContractId == id)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new
                {
                    a.Id,
                    a.FileName,
                    a.FileType,
                    a.UploadedAt,
                    a.UploadedBy
                })
                .ToListAsync();

            return Ok(attachments);
        }

        [HttpGet("{id}/attachments/{attachmentId}/download")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            var attachment = await _context.ContractAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.HealthContractId == id);

            if (attachment == null) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound("File không tồn tại trên server");

            var mimeType = attachment.FileType.ToLower() switch
            {
                "pdf" => "application/pdf",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, mimeType, attachment.FileName);
        }

        [HttpDelete("{id}/attachments/{attachmentId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Upload")]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            var attachment = await _context.ContractAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.HealthContractId == id);

            if (attachment == null) return NotFound();

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.ContractAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa file đính kèm thành công" });
        }

        [HttpDelete("{id}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var (success, error) = await _contractService.DeleteContractAsync(id);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }
            return Ok(new { message = "Xóa hợp đồng thành công." });
        }

        [HttpPut("{id}/lock")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> LockContract(int id)
        {
            var username = User.Identity?.Name ?? "system";
            var (success, error) = await _contractService.LockContractAsync(id, username);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }
            return Ok(new { message = "Hợp đồng đã được khóa." });
        }

        [HttpPut("{id}/unlock")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> UnlockContract(int id)
        {
            var username = User.Identity?.Name ?? "system";
            var (success, error) = await _contractService.UnlockContractAsync(id, username);
            if (!success) {
                if (error == "Not Found") return NotFound();
                return BadRequest(error);
            }
            return Ok(new { message = "Đã mở khóa hợp đồng." });
        }

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

        [HttpGet("{id}/settlement")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.View")]
        public async Task<IActionResult> GetSettlement(int id)
        {
            var result = await _settlementService.CalculateSettlementAsync(id);
            if (result == null) return NotFound("Hợp đồng không tồn tại.");
            return Ok(result);
        }

        [HttpPost("{id}/costs/settle")]
        [AuthorizePermission("QuyetToan.Finalize")] // Phase 1: Only Accountant or Admin can finalize
        public async Task<IActionResult> PostSettleCost(int id, [FromBody] string note)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var snapshot = await _costCalculationService.CalculateAndSaveSnapshotAsync(id, "SETTLE", note ?? "Quyết toán chủ động", userId);

                var contract = await _context.Contracts.FindAsync(id);
                if (contract != null)
                {
                    var oldStatus = contract.Status.ToString();
                    contract.Status = ContractStatus.Finished;
                    
                    _context.ContractStatusHistories.Add(new ContractStatusHistory
                    {
                        HealthContractId = id, 
                        OldStatus = oldStatus, 
                        NewStatus = ContractStatus.Finished.ToString(),
                        ChangedBy = User.Identity?.Name ?? "system", 
                        ChangedAt = DateTime.Now, 
                        Note = "Đã chốt quyết toán chi phí"
                    });
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Đã lưu bản ghi Quyết toán.", snapshotId = snapshot.SnapshotId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi khi chốt quyết toán. Đã rollback toàn bộ. Chi tiết: " + ex.Message });
            }
        }

        [HttpPut("{id}/extra-service")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HopDong.Edit")]
        public async Task<IActionResult> UpdateExtraService(int id, [FromBody] ContractExtraServiceDto request)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound("Hợp đồng không tồn tại.");

            contract.ExtraServiceRevenue = request.ExtraServiceRevenue;
            contract.ExtraServiceDetails = request.ExtraServiceDetails;
            contract.VATRate = request.VATRate;
            
            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id,
                OldStatus = contract.Status.ToString(),
                NewStatus = contract.Status.ToString(),
                ChangedBy = User.Identity?.Name ?? "system",
                ChangedAt = DateTime.Now,
                Note = $"Cập nhật doanh thu ngoài gói: {request.ExtraServiceRevenue:N0} VNĐ"
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật doanh thu ngoài gói thành công.", value = request });
        }

        [HttpGet("{id}/costs/snapshots")]
        [AuthorizePermission("QuyetToan.Calculate")] // Phase 1: Cost snapshots require Calculate permission
        public async Task<IActionResult> GetSnapshots(int id)
        {
            var snapshots = await _context.Set<CostSnapshot>()
                .Where(s => s.HealthContractId == id)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new {
                    s.SnapshotId,
                    s.SnapshotType,
                    s.Revenue,
                    s.TotalCost,
                    s.GrossProfit,
                    s.CreatedAt,
                    s.Note,
                    CreatedByName = s.CreatedByUser != null ? s.CreatedByUser.FullName : null
                })
                .ToListAsync();
            return Ok(snapshots);
        }

        [HttpGet("{id}/pnl-report")]
        [AuthorizePermission("BaoCao.ViewFinance")] // Phase 3: Requires high-level permission
        public async Task<IActionResult> GetPnlReport(int id)
        {
            try
            {
                var report = await _financialEngine.CalculatePnlAsync(id);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/create-medical-groups")]
        [AuthorizePermission("DoanKham.Create")]
        public async Task<IActionResult> AutoCreateMedicalGroups(int id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

            try
            {
                var autoCreateService = new AutoCreateMedicalGroupService(_context);

                // Kiểm tra xem hợp đồng đã được duyệt chưa
                var canCreate = await autoCreateService.CanCreateMedicalGroups(id);
                if (!canCreate)
                {
                    return BadRequest(new
                    {
                        message = "Hợp đồng chưa được duyệt. Vui lòng duyệt hợp đồng trước khi tạo đoàn khám."
                    });
                }

                var groups = await autoCreateService.CreateMedicalGroupsFromContractAsync(id, userId);

                return Ok(new
                {
                    message = $"Đã tạo {groups.Count} đoàn khám từ hợp đồng",
                    count = groups.Count,
                    groups = groups.Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        g.ExamDate,
                        g.Status
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/medical-groups")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetContractMedicalGroups(int id)
        {
            var groups = await _context.MedicalGroups
                .Where(mg => mg.HealthContractId == id)
                .OrderBy(mg => mg.ExamDate)
                .Select(mg => new
                {
                    mg.GroupId,
                    mg.GroupName,
                    mg.ExamDate,
                    mg.Status,
                    mg.Slot,
                    mg.StartTime,
                    mg.DepartureTime,
                    StaffCount = mg.StaffDetails.Count,
                    PatientCount = _context.MedicalRecords.Count(mr => mr.GroupId == mg.GroupId)
                })
                .ToListAsync();

            return Ok(groups);
        }
    }
}
