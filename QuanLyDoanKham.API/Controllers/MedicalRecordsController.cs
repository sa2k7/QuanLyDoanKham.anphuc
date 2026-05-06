using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services;
using QuanLyDoanKham.API.Services.MedicalRecords;
using ClosedXML.Excel;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IMedicalReportPdfGenerator _pdfGenerator;
        private readonly IMedicalRecordStateMachine _stateMachine;
        private readonly ICheckInService _checkInService;
        private readonly ApplicationDbContext _context;

        public MedicalRecordsController(
            IMedicalRecordService medicalRecordService, 
            IMedicalReportPdfGenerator pdfGenerator,
            IMedicalRecordStateMachine stateMachine,
            ICheckInService checkInService,
            ApplicationDbContext context)
        {
            _medicalRecordService = medicalRecordService;
            _pdfGenerator = pdfGenerator;
            _stateMachine = stateMachine;
            _checkInService = checkInService;
            _context = context;
        }

        // ================================================================
        // GET: api/MedicalRecords/all — Tất cả bệnh án (phân trang, lọc)
        // ================================================================
        [HttpGet("all")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? contractId,
            [FromQuery] int? groupId,
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.MedicalRecords
                .Include(m => m.MedicalGroup)
                    .ThenInclude(g => g!.HealthContract)
                        .ThenInclude(c => c!.Company)
                .AsQueryable();

            if (contractId.HasValue)
                query = query.Where(m => m.MedicalGroup != null && m.MedicalGroup.HealthContractId == contractId.Value);

            if (groupId.HasValue)
                query = query.Where(m => m.GroupId == groupId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(m => m.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m =>
                    m.FullName.ToLower().Contains(s) ||
                    (m.IDCardNumber != null && m.IDCardNumber.Contains(s)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.MedicalRecordId,
                    m.FullName,
                    m.DateOfBirth,
                    m.Gender,
                    m.IDCardNumber,
                    m.Department,
                    m.Status,
                    m.GroupId,
                    GroupName = m.MedicalGroup != null ? m.MedicalGroup.GroupName : null,
                    ExamDate = m.MedicalGroup != null ? m.MedicalGroup.ExamDate : (DateTime?)null,
                    ContractId = m.MedicalGroup != null ? m.MedicalGroup.HealthContractId : (int?)null,
                    ContractName = m.MedicalGroup != null && m.MedicalGroup.HealthContract != null
                        ? m.MedicalGroup.HealthContract.ContractName : null,
                    CompanyName = m.MedicalGroup != null && m.MedicalGroup.HealthContract != null && m.MedicalGroup.HealthContract.Company != null
                        ? m.MedicalGroup.HealthContract.Company.CompanyName : null,
                    m.CheckInAt,
                    m.CreatedAt
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // ================================================================
        // PUT: api/MedicalRecords/{id}/basic — Sửa thông tin cơ bản bệnh án
        // ================================================================
        [HttpPut("{id}/basic")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> UpdateBasicInfo(int id, [FromBody] MedicalRecordBasicUpdateDto dto)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound(new { message = "Không tìm thấy bệnh án." });

            record.FullName = dto.FullName?.Trim() ?? record.FullName;
            record.Gender = dto.Gender;
            record.DateOfBirth = dto.DateOfBirth;
            record.IDCardNumber = dto.IDCardNumber?.Trim();
            record.Department = dto.Department?.Trim();
            record.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật bệnh án thành công." });
        }

        // ================================================================
        // GET: api/MedicalRecords/export — Xuất Excel bệnh án
        // ================================================================
        [HttpGet("export")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] int? contractId,
            [FromQuery] int? groupId,
            [FromQuery] string? search)
        {
            var query = _context.MedicalRecords
                .Include(m => m.MedicalGroup)
                    .ThenInclude(g => g!.HealthContract)
                .AsQueryable();

            if (contractId.HasValue)
                query = query.Where(m => m.MedicalGroup != null && m.MedicalGroup.HealthContractId == contractId.Value);
            if (groupId.HasValue)
                query = query.Where(m => m.GroupId == groupId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m => m.FullName.ToLower().Contains(s) ||
                    (m.IDCardNumber != null && m.IDCardNumber.Contains(s)));
            }

            var data = await query.OrderBy(m => m.FullName).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DanhSachBenhAn");

            ws.Cell(1, 1).Value = "STT";
            ws.Cell(1, 2).Value = "Họ và Tên";
            ws.Cell(1, 3).Value = "Giới tính";
            ws.Cell(1, 4).Value = "Ngày sinh";
            ws.Cell(1, 5).Value = "CCCD/CMND";
            ws.Cell(1, 6).Value = "Phòng ban";
            ws.Cell(1, 7).Value = "Đoàn khám";
            ws.Cell(1, 8).Value = "Trạng thái";
            ws.Cell(1, 9).Value = "Ngày tạo";

            var headerRange = ws.Range(1, 1, 1, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < data.Count; i++)
            {
                var m = data[i];
                ws.Cell(i + 2, 1).Value = i + 1;
                ws.Cell(i + 2, 2).Value = m.FullName;
                ws.Cell(i + 2, 3).Value = m.Gender;
                ws.Cell(i + 2, 4).Value = m.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(i + 2, 5).Value = m.IDCardNumber;
                ws.Cell(i + 2, 6).Value = m.Department;
                ws.Cell(i + 2, 7).Value = m.MedicalGroup?.GroupName;
                ws.Cell(i + 2, 8).Value = m.Status;
                ws.Cell(i + 2, 9).Value = m.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DanhSachBenhAn_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // POST: api/MedicalRecords/batch-ingest
        [HttpPost("batch-ingest")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> BatchIngest([FromBody] MedicalRecordBatchIngestRequestDto request)
        {
            var username = User.Identity?.Name ?? "system";
            var result = await _medicalRecordService.BatchIngestAsync(request, username);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { 
                message = "Đã nhập thành công hồ sơ y tế.", 
                count = result.Data?.Count ?? 0,
                groupId = request.GroupId
            });
        }

        // GET: api/MedicalRecords/by-group/{groupId}
        [HttpGet("by-group/{groupId}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<ActionResult<IEnumerable<object>>> GetByGroup(int groupId)
        {
            var records = await _medicalRecordService.GetByGroupAsync(groupId);
            return Ok(records);
        }

        // POST: api/MedicalRecords/checkin-token
        [HttpPost("checkin-token")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckInByToken([FromBody] QrCheckInDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.QrToken))
                return BadRequest(new { message = "QR token không hợp lệ." });

            var actor = User.Identity?.Name ?? "SYSTEM";
            var result = await _checkInService.ProcessCheckInTokenAsync(dto.QrToken, actor);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }

        // POST: api/MedicalRecords/batch-ingest-excel
        [HttpPost("batch-ingest-excel")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> BatchIngestExcel([FromQuery] int groupId, [FromQuery] string filePath)
        {
            var username = User.Identity?.Name ?? "system";
            var result = await _medicalRecordService.BatchIngestFromExcelAsync(groupId, filePath, username);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Đã nhập dữ liệu từ Excel thành công.", count = result.Data?.Count ?? 0 });
        }

        // DELETE: api/MedicalRecords/{id}
        [HttpDelete("{id}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicalRecordService.DeleteAsync(id);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Đã xóa hồ sơ y tế thành công." });
        }

        // GET: api/MedicalRecords/{id}
        [HttpGet("{id}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _medicalRecordService.GetByIdAsync(id);
            if (record == null) return NotFound();
            return Ok(record);
        }


        // GET: api/MedicalRecords/{id}/export-pdf
        [HttpGet("{id}/export-pdf")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            try
            {
                var pdfBytes = await _pdfGenerator.GenerateReportAsync(id);
                return File(pdfBytes, "application/pdf", $"KhamSucKhoe_{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/MedicalRecords/{id}/ai-clinical-summary
        [HttpPost("{id}/ai-clinical-summary")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> GetAiClinicalSummary(int id)
        {
            var result = await _medicalRecordService.GenerateAiClinicalSummaryAsync(id);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { summary = result.Data });
        }

        // ─── Lifecycle Endpoints (Simplified) ────────────────────────────────

        [HttpPost("{id}/checkin")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.CheckInAsync(id, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });

            var record = result.Data;
            var patient = record != null
                ? await _medicalRecordService.GetByIdAsync(record.MedicalRecordId)
                : null;
            int queueNo = 1;
            if (record != null)
            {
                var today = DateTime.Today;
                queueNo = await _context.MedicalRecords
                    .Where(m => m.GroupId == record.GroupId && m.CheckInAt != null && m.CheckInAt.Value.Date == today)
                    .CountAsync();
                if (queueNo == 0) queueNo = 1; // Just in case, though CheckInAsync already saves CheckInAt
            }

            return Ok(new
            {
                message = "Tiếp đón thành công",
                data = new
                {
                    medicalRecordId = record?.MedicalRecordId,
                    fullName = patient?.FullName ?? "Không rõ",
                    queueNo = queueNo.ToString().PadLeft(3, '0'),
                    serviceName = "Khám tổng quát",
                    status = record?.Status?.ToString(),
                    checkInAt = record?.CheckInAt
                }
            });
        }

        [HttpPost("{id}/finalize")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> FinalizeRecord(int id)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.FinalizeRecordAsync(id, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpPost("{id}/qc-pass")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> QcPass(int id)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.QCPassAsync(id, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Hồ sơ đã được duyệt QC thành công." });
        }

        [HttpPost("{id}/qc-rework")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> QcRework(int id, [FromBody] string reason)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.QCReworkAsync(id, userId, reason);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Đã trả hồ sơ về bác sĩ để chỉnh sửa." });
        }

        [HttpPost("{id}/cancel")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CancelRecord(int id, [FromBody] string reason)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.CancelRecordAsync(id, userId, reason);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Hồ sơ đã được hủy thành công." });
        }

        [HttpGet("qc-pending")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> GetQcPending()
        {
            var records = await _medicalRecordService.GetQcPendingRecordsAsync();
            return Ok(records);
        }
    }

    // DTO for basic info update
    public class MedicalRecordBasicUpdateDto
    {
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? IDCardNumber { get; set; }
        public string? Department { get; set; }
    }
}
