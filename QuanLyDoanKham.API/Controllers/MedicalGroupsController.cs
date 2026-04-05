using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.MedicalGroups;
using System.Text;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalGroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.IGeminiService _geminiService;
        private readonly IMedicalGroupAutoAssignmentService _autoAssignmentService;

        public MedicalGroupsController(ApplicationDbContext context, Services.IGeminiService geminiService, IMedicalGroupAutoAssignmentService autoAssignmentService)
        {
            _context = context;
            _geminiService = geminiService;
            _autoAssignmentService = autoAssignmentService;
        }

        // GET: api/MedicalGroups
        [HttpGet]
        [AuthorizePermission("DoanKham.View")]
        public async Task<ActionResult<IEnumerable<MedicalGroupDto>>> GetMedicalGroups()
        {
            var today = DateTime.Today;
            return await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .OrderByDescending(g => g.GroupId)
                .Select(g => new MedicalGroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    ExamDate = g.ExamDate,
                    HealthContractId = g.HealthContractId,
                    ShortName = g.HealthContract.Company.ShortName,
                    CompanyName = g.HealthContract.Company.CompanyName,
                    Status = g.Status,
                    ImportFilePath = g.ImportFilePath
                })
                .ToListAsync();
        }

        // GET: api/MedicalGroups/calendar?from=2024-01-01&to=2024-01-31
        [HttpGet("calendar")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("LichKham.ViewAll")]
        public async Task<IActionResult> GetCalendar([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var start = from?.Date ?? DateTime.Today.AddDays(-7);
            var end = to?.Date ?? DateTime.Today.AddDays(30);
            var data = await _context.MedicalGroups
                .Where(g => g.ExamDate.Date >= start && g.ExamDate.Date <= end)
                .Select(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    g.ExamDate,
                    g.Status,
                    g.HealthContractId
                }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}/qr")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.QR")]
        public async Task<IActionResult> GetQr(int id, [FromServices] QuanLyDoanKham.API.Services.QrService qr)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();
            // Payload đơn giản cũ để giữ tương thích hoặc debug
            var png = qr.GenerateQr($"GROUP:{id}|DATE:{group.ExamDate:yyyy-MM-dd}");
            return Ok(new { pngBase64 = png });
        }

        // POST: api/MedicalGroups
        [HttpPost]
        [AuthorizePermission("DoanKham.Create")]
        public async Task<ActionResult<MedicalGroup>> PostMedicalGroup(MedicalGroupDto dto)
        {
            var contract = await _context.Contracts.FindAsync(dto.HealthContractId);
            if (contract == null) return NotFound("Không tìm thấy hợp đồng.");
            
            if (contract.Status != "Active" && contract.Status != "Approved") 
                return BadRequest("Hợp đồng chưa được phê duyệt hoặc đã kết thúc. Chỉ hợp đồng 'Approved' hoặc 'Active' mới được tạo đoàn khám.");

            var entity = new MedicalGroup
            {
                GroupName = dto.GroupName,
                ExamDate = dto.ExamDate,
                HealthContractId = dto.HealthContractId,
                Status = "Open",
                ImportFilePath = dto.ImportFilePath
            };

            _context.MedicalGroups.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        // PUT: api/MedicalGroups/{id}
        [HttpPut("{id}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> PutMedicalGroup(int id, MedicalGroupDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            if (group.Status == "Locked") return BadRequest("Đoàn khám đã bị khóa.");

            group.GroupName = dto.GroupName;
            group.ExamDate = dto.ExamDate;
            group.Status = dto.Status;
            group.ImportFilePath = dto.ImportFilePath;

            await _context.SaveChangesAsync();
            return Ok(group);
        }

        // POST: api/MedicalGroups/generate-from-contract/{contractId}
        [HttpPost("generate-from-contract/{contractId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("DoanKham.Create")]
        public async Task<IActionResult> GenerateFromContract(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.HealthContractId == contractId);
            if (contract == null) return NotFound();
            if (contract.Status != "Approved" && contract.Status != "Active")
                return BadRequest("Hợp đồng chưa được duyệt hoặc chưa kích hoạt.");

            var start = contract.StartDate.Date;
            var end = contract.EndDate.Date;
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var exists = await _context.MedicalGroups.AnyAsync(g =>
                    g.HealthContractId == contractId && g.ExamDate.Date == d);
                if (exists) continue;

                _context.MedicalGroups.Add(new MedicalGroup
                {
                    GroupName = $"{contract.Company?.ShortName ?? contract.Company?.CompanyName} - {d:dd/MM/yyyy}",
                    ExamDate = d,
                    HealthContractId = contractId,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "system"
                });
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã sinh đoàn theo dải ngày hợp đồng." });
        }

        // PUT: api/MedicalGroups/{id}/status
        [HttpPut("{id}/status")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            // Nếu đã khóa thì không cho đổi nữa
            if (group.Status == "Locked")
                return BadRequest("Đoàn khám đã bị khóa, không thể thay đổi trạng thái.");

            group.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công", status = group.Status });
        }

        // GET: api/MedicalGroups/my-schedule — MedicalStaff xem lịch đi đoàn cá nhân
        [HttpGet("my-schedule")]
        [AuthorizePermission("LichKham.ViewOwn")]
        public async Task<IActionResult> GetMySchedule()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            // Tìm Staff tương ứng với tài khoản đang đăng nhập
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode.ToLower() == username.ToLower());
            if (staff == null) return NotFound(new { message = "Không tìm thấy hồ sơ nhân sự liên kết với tài khoản này." });

            var schedule = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                    .ThenInclude(g => g.HealthContract)
                        .ThenInclude(c => c.Company)
                .Where(gsd => gsd.StaffId == staff.StaffId)
                .OrderByDescending(gsd => gsd.MedicalGroup.ExamDate)
                .Select(gsd => new
                {
                    gsd.Id,
                    gsd.MedicalGroup.GroupId,
                    gsd.MedicalGroup.GroupName,
                    gsd.MedicalGroup.ExamDate,
                    CompanyName = gsd.MedicalGroup.HealthContract.Company.ShortName ?? gsd.MedicalGroup.HealthContract.Company.CompanyName,
                    gsd.WorkPosition,
                    gsd.WorkStatus,
                    gsd.ShiftType,
                    gsd.CheckInTime,
                    gsd.CheckOutTime,
                    GroupStatus = gsd.MedicalGroup.Status
                })
                .ToListAsync();

            return Ok(schedule);
        }

        // GET: api/MedicalGroups/{id}/staffs
        [HttpGet("{id}/staffs")]
        public async Task<ActionResult<IEnumerable<GroupStaffItemDto>>> GetGroupStaffs(int id)
        {
            return await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.GroupId == id)
                .Select(gsd => new GroupStaffItemDto
                {
                    Id = gsd.Id,
                    StaffId = gsd.StaffId,
                    EmployeeCode = gsd.Staff.EmployeeCode,
                    FullName = gsd.Staff.FullName,
                    JobTitle = gsd.Staff.JobTitle,
                    ShiftType = gsd.ShiftType,
                    CalculatedSalary = gsd.CalculatedSalary,
                    WorkPosition = gsd.WorkPosition,
                    PositionId = gsd.PositionId,
                    WorkStatus = gsd.WorkStatus
                })
                .ToListAsync();
        }

        // POST: api/MedicalGroups/{id}/staffs
        [HttpPost("{id}/staffs")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("DoanKham.AssignStaff")]
        public async Task<IActionResult> AddStaffToGroup(int id, [FromBody] AddStaffToGroupDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound("Đoàn khám không tồn tại.");
            if (group.Status == "Locked" || group.Status == "Finished") 
                return BadRequest("Đoàn khám đã đóng hoặc khóa.");

            var staff = await _context.Staffs.FindAsync(dto.StaffId);
            if (staff == null) return NotFound("Nhân viên không tồn tại.");

            // Check chuyên môn/department với vị trí yêu cầu
            if (dto.PositionId.HasValue)
            {
                var quota = await _context.GroupPositionQuotas
                    .Include(q => q.Position)
                    .FirstOrDefaultAsync(q => q.Id == dto.PositionId.Value || q.PositionId == dto.PositionId.Value && q.MedicalGroupId == id);
                if (quota != null && !string.IsNullOrEmpty(quota.Position.SpecialtyRequired))
                {
                    if (!string.Equals(quota.Position.SpecialtyRequired, staff.Specialty, StringComparison.OrdinalIgnoreCase))
                        return BadRequest("Sai chuyên môn so với vị trí yêu cầu.");
                }
                // Kiểm tra quota
                if (quota != null && quota.Assigned >= quota.Required)
                    return BadRequest("Vị trí đã đủ định biên.");
            }

            // Kiểm tra trùng lịch theo ngày + ca (ShiftType)
            var examDate = group.ExamDate.Date;
            var isOverlapping = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                .AnyAsync(gsd => gsd.StaffId == dto.StaffId
                    && gsd.MedicalGroup.ExamDate.Date == examDate
                    && Math.Abs(gsd.ShiftType - dto.ShiftType) < 0.001);

            if (isOverlapping)
                return BadRequest($"Nhân viên {staff.FullName} đã được phân công vào đoàn khác trong ngày {examDate:dd/MM/yyyy}.");

            var detail = new GroupStaffDetail
            {
                GroupId = id,
                StaffId = dto.StaffId,
                ShiftType = dto.ShiftType,
                CalculatedSalary = staff.BaseSalary * (decimal)dto.ShiftType,
                ExamDate = examDate,
                WorkPosition = dto.WorkPosition,
                PositionId = dto.PositionId,
                GroupPositionQuotaId = dto.PositionId,
                WorkStatus = dto.WorkStatus ?? "Đang chờ",
                AssignedByUserId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null,
                AssignedAt = DateTime.UtcNow
            };

            // Cập nhật AssignedCount cho quota nếu có
            if (dto.PositionId.HasValue)
            {
                var quota = await _context.GroupPositionQuotas.FindAsync(dto.PositionId.Value);
                if (quota != null)
                {
                    quota.Assigned = Math.Min(quota.Assigned + 1, quota.Required);
                }
            }

            _context.GroupStaffDetails.Add(detail);
            await _context.SaveChangesAsync();

            // TẠO THÔNG BÁO NỘI BỘ
            try
            {
                var userAccount = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == staff.EmployeeCode.ToLower());
                if (userAccount != null)
                {
                    var notification = new Notification
                    {
                        UserId = userAccount.UserId,
                        Message = $"Bạn đã được điều động tham gia đoàn khám '{group.GroupName}' vào ngày {group.ExamDate:dd/MM/yyyy}. Vị trí: {dto.WorkPosition}.",
                        Link = $"/groups?id={group.GroupId}",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Swallow error to not block the main transaction if notification fails
            }

            return Ok(detail);
        }

        // POST: api/MedicalGroups/auto-create-with-staff
        [HttpPost("auto-create-with-staff")]
        [AuthorizePermission("DoanKham.Create")]
        public async Task<IActionResult> AutoCreateWithStaff([FromBody] AutoCreateGroupWithStaffRequestDto request)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _autoAssignmentService.AutoCreateAndAssignAsync(request, userId);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }

        // POST: api/MedicalGroups/auto-create/{contractId}
        [HttpPost("auto-create/{contractId}")]
        [AuthorizePermission("DoanKham.Create")]
        public async Task<ActionResult<MedicalGroup>> AutoCreateFromContract(int contractId)
        {
            var contract = await _context.Contracts.Include(c => c.Company).FirstOrDefaultAsync(c => c.HealthContractId == contractId);
            if (contract == null) return NotFound("Hợp đồng không khả dụng.");
            if (contract.Status != "Active" && contract.Status != "Approved") return BadRequest("Hợp đồng chưa được phê duyệt.");

            var existingGroupsCount = await _context.MedicalGroups.CountAsync(g => g.HealthContractId == contractId);
            
            var newGroup = new MedicalGroup
            {
                GroupName = $"Đoàn khám {contract.Company.ShortName ?? contract.Company.CompanyName} - Lần {existingGroupsCount + 1}",
                ExamDate = DateTime.Now.AddDays(7), // Mặc định 7 ngày sau
                HealthContractId = contractId,
                Status = "Open",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "system"
            };

            _context.MedicalGroups.Add(newGroup);
            await _context.SaveChangesAsync();
            return Ok(newGroup);
        }

        // DELETE: api/MedicalGroups/staffs/{detailId}
        [HttpDelete("staffs/{detailId}")]
        [AuthorizePermission("DoanKham.AssignStaff")]
        public async Task<IActionResult> RemoveStaffFromGroup(int detailId)
        {
            var detail = await _context.GroupStaffDetails.FindAsync(detailId);
            if (detail == null) return NotFound();

            var group = await _context.MedicalGroups.FindAsync(detail.GroupId);
            if (group != null && (group.Status == "Locked" || group.Status == "Finished"))
                return BadRequest("Đoàn khám đã đóng hoặc khóa.");

            if (detail.GroupPositionQuotaId.HasValue)
            {
                var quota = await _context.GroupPositionQuotas.FindAsync(detail.GroupPositionQuotaId.Value);
                if (quota != null && quota.Assigned > 0) quota.Assigned -= 1;
            }

            _context.GroupStaffDetails.Remove(detail);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PATCH: api/MedicalGroups/staffs/{detailId}/status
        // Cap nhat trang thai diem danh (Dang cho / Da tham gia / Vang mat / Xin nghi)
        [HttpPatch("staffs/{detailId}/status")]
        [AuthorizePermission("DoanKham.AssignStaff")]
        public async Task<IActionResult> UpdateWorkStatus(int detailId, [FromBody] UpdateWorkStatusDto dto)
        {
            var detail = await _context.GroupStaffDetails.FindAsync(detailId);
            if (detail == null) return NotFound("Không tìm thấy bản ghi phân công.");

            var validStatuses = new[] { "Đang chờ", "Đã tham gia", "Vắng mặt", "Xin nghỉ" };
            if (!validStatuses.Contains(dto.WorkStatus))
                return BadRequest($"Trạng thái '{dto.WorkStatus}' không hợp lệ.");

            detail.WorkStatus = dto.WorkStatus;
            if (!string.IsNullOrEmpty(dto.Note)) detail.Note = dto.Note;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật trạng thái.", workStatus = detail.WorkStatus });
        }

        // POST: api/MedicalGroups/staffs/{detailId}/checkin
        // Ghi nhan gio check-in thuc te — Admin, GroupMgr hoặc MedicalStaff (tự check-in)
        [HttpPost("staffs/{detailId}/checkin")]
        [AuthorizePermission("ChamCong.CheckInOut")]
        public async Task<IActionResult> CheckIn(int detailId)
        {
            var detail = await _context.GroupStaffDetails.Include(d => d.Staff).FirstOrDefaultAsync(d => d.Id == detailId);
            if (detail == null) return NotFound("Không tìm thấy bản ghi phân công.");

            // MedicalStaff chỉ được tự check-in cho chính mình
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (userRole == "MedicalStaff")
            {
                var username = User.Identity?.Name;
                var myStaff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode.ToLower() == username.ToLower());
                if (myStaff == null || myStaff.StaffId != detail.StaffId)
                    return StatusCode(403, new { message = "Bạn chỉ có thể check-in cho chính mình." });
            }

            if (detail.CheckInTime.HasValue)
                return BadRequest($"Nhân viên này đã check-in lúc {detail.CheckInTime:HH:mm dd/MM/yyyy}.");

            detail.CheckInTime = DateTime.Now;
            detail.WorkStatus = "Đã tham gia";

            // Ghi Audit Log
            var currentUsername = User.Identity?.Name ?? "system";
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);
            if (currentUser != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = currentUser.UserId,
                    Action = "CHECK_IN",
                    EntityType = "GroupStaffDetail",
                    EntityId = detailId,
                    OldValue = null,
                    NewValue = $"Staff {detail.Staff?.FullName} check-in lúc {detail.CheckInTime:HH:mm}",
                    Timestamp = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Check-in thành công!", checkInTime = detail.CheckInTime });
        }

        // POST: api/MedicalGroups/staffs/{detailId}/checkout
        // Ghi nhan gio check-out khi ket thuc buoi kham
        [HttpPost("staffs/{detailId}/checkout")]
        [AuthorizePermission("ChamCong.CheckInOut")]
        public async Task<IActionResult> CheckOut(int detailId)
        {
            var detail = await _context.GroupStaffDetails.Include(d => d.Staff).FirstOrDefaultAsync(d => d.Id == detailId);
            if (detail == null) return NotFound("Không tìm thấy bản ghi phân công.");

            // MedicalStaff chỉ được tự check-out cho chính mình
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (userRole == "MedicalStaff")
            {
                var username = User.Identity?.Name;
                var myStaff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode.ToLower() == username.ToLower());
                if (myStaff == null || myStaff.StaffId != detail.StaffId)
                    return StatusCode(403, new { message = "Bạn chỉ có thể check-out cho chính mình." });
            }

            if (!detail.CheckInTime.HasValue)
                return BadRequest("Chưa có check-in. Không thể check-out.");

            if (detail.CheckOutTime.HasValue)
                return BadRequest($"Nhân viên này đã check-out lúc {detail.CheckOutTime:HH:mm dd/MM/yyyy}.");

            detail.CheckOutTime = DateTime.Now;

            // Ghi Audit Log
            var currentUsername = User.Identity?.Name ?? "system";
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);
            if (currentUser != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = currentUser.UserId,
                    Action = "CHECK_OUT",
                    EntityType = "GroupStaffDetail",
                    EntityId = detailId,
                    OldValue = null,
                    NewValue = $"Staff {detail.Staff?.FullName} check-out lúc {detail.CheckOutTime:HH:mm}",
                    Timestamp = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
            }

            await _context.SaveChangesAsync();

            var duration = detail.CheckOutTime.Value - detail.CheckInTime.Value;
            return Ok(new { 
                message = "Check-out thành công!", 
                checkOutTime = detail.CheckOutTime,
                totalHours = Math.Round(duration.TotalHours, 1)
            });
        }


        // POST: api/MedicalGroups/upload-data
        [HttpPost("upload-data")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> UploadData([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "groups");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { path = $"uploads/groups/{fileName}" });
        }
        // GET: api/MedicalGroups/export
        [HttpGet("export")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportGroupsExcel()
        {
            var groups = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachDoanKham");
                worksheet.Cell(1, 1).Value = "Tên Đoàn Khám";
                worksheet.Cell(1, 2).Value = "Khách Hàng";
                worksheet.Cell(1, 3).Value = "Ngày Khám";
                worksheet.Cell(1, 4).Value = "Trạng Thái";
                
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.AliceBlue;

                for (int i = 0; i < groups.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = groups[i].GroupName;
                    worksheet.Cell(i + 2, 2).Value = groups[i].HealthContract.Company.ShortName ?? groups[i].HealthContract.Company.CompanyName;
                    worksheet.Cell(i + 2, 3).Value = groups[i].ExamDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(i + 2, 4).Value = groups[i].Status;
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDoanKham.xlsx");
                }
            }
        }

        // GET: api/MedicalGroups/{id}/export-staff
        [HttpGet("{id}/export-staff")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportGroupStaffExcel(int id)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            var staff = await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.GroupId == id)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DoiNguDiKham");
                worksheet.Cell(1, 1).Value = $"DANH SÁCH NHÂN SỰ: {group.GroupName}";
                worksheet.Range(1, 1, 1, 5).Merge().Style.Font.Bold = true;
                
                worksheet.Cell(3, 1).Value = "Mã NV";
                worksheet.Cell(3, 2).Value = "Họ và Tên";
                worksheet.Cell(3, 3).Value = "Vị trí tại đoàn";
                worksheet.Cell(3, 4).Value = "Cấp bậc";
                worksheet.Cell(3, 5).Value = "Ca làm";

                worksheet.Range(3, 1, 3, 5).Style.Font.Bold = true;

                for (int i = 0; i < staff.Count; i++)
                {
                    worksheet.Cell(i + 4, 1).Value = staff[i].Staff.EmployeeCode;
                    worksheet.Cell(i + 4, 2).Value = staff[i].Staff.FullName;
                    worksheet.Cell(i + 4, 3).Value = staff[i].WorkPosition;
                    worksheet.Cell(i + 4, 4).Value = staff[i].Staff.JobTitle;
                    worksheet.Cell(i + 4, 5).Value = staff[i].ShiftType == 1 ? "Cả ngày" : "Nửa ngày";
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NhanSu_{group.GroupName}.xlsx");
                }
            }
        }

        // POST: api/MedicalGroups/{id}/ai-suggest-staff
        [HttpPost("{id}/ai-suggest-staff")]
        [AuthorizePermission("DoanKham.AssignStaff")]
        public async Task<IActionResult> AiSuggestStaff(int id)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null) return NotFound("Đoàn khám không tồn tại.");

            // 1. Thu thập dữ liệu ngữ cảnh
            var contract = group.HealthContract;
            var expectedPeople = contract.ExpectedQuantity;
            
            // Lấy danh sách nhân sự rảnh trong ngày hôm đó (hoặc tất cả nhân sự để AI chọn)
            var allStaff = await _context.Staffs.ToListAsync();
            var busyStaffIds = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                .Where(gsd => gsd.MedicalGroup.ExamDate.Date == group.ExamDate.Date && gsd.GroupId != id)
                .Select(gsd => gsd.StaffId)
                .ToListAsync();

            var availableStaff = allStaff
                .Select(s => new {
                    s.StaffId,
                    s.FullName,
                    s.StaffType, // BacSi, DieuDuong, KyThuatVien, Khac
                    s.JobTitle,
                    IsBusy = busyStaffIds.Contains(s.StaffId)
                })
                .ToList();

            // 2. Xây dựng Prompt cho Gemini
            var prompt = new StringBuilder();
            prompt.AppendLine("Bạn là một chuyên gia điều phối nhân sự y tế. Hãy phân bổ nhân sự cho đoàn khám sau:");
            prompt.AppendLine($"- Tên đoàn: {group.GroupName}");
            prompt.AppendLine($"- Quy mô: {expectedPeople} người khám");
            prompt.AppendLine($"- Ngày khám: {group.ExamDate:dd/MM/yyyy}");
            prompt.AppendLine("\nDanh sách nhân sự khả dụng (StaffType: BacSi, DieuDuong, KyThuatVien, Khac):");
            foreach (var s in availableStaff) {
                prompt.AppendLine($"- ID: {s.StaffId}, Tên: {s.FullName}, Loại: {s.StaffType}, Chức vụ: {s.JobTitle} {(s.IsBusy ? "[ĐÃ CÓ LỊCH]" : "")}");
            }

            prompt.AppendLine("\nYêu cầu phân bổ:");
            prompt.AppendLine("1. MUST: Tổng số nhân sự khoảng 1:15 hoặc 1:20 so với quy mô người khám.");
            prompt.AppendLine("2. MUST: Phải có ít nhất 2 BacSi (vị trí: Khám nội, Khám ngoại, Siêu âm, Sản phụ khoa). Bác sĩ CẤM làm Tiếp nhận/Hậu cần.");
            prompt.AppendLine("3. Vị trí Tiếp nhận & Phân loại: Thường là DieuDuong hoặc Khac.");
            prompt.AppendLine("4. Vị trí Cân đo & Huyết áp: DieuDuong.");
            prompt.AppendLine("5. Vị trí Lấy máu: DieuDuong hoặc KyThuatVien.");
            prompt.AppendLine("6. Vị trí Hậu cần: DieuDuong hoặc Khac.");
            prompt.AppendLine("\nKết quả trả về DUY NHẤT một mảng JSON format: [{\"staffId\": int, \"workPosition\": string, \"shiftType\": float, \"reason\": string}]");
            prompt.AppendLine("Vị trí (workPosition) phải là một trong: Tiếp nhận, Cân đo huyết áp, Khám nội, Khám ngoại, Lấy máu, Siêu âm, Khám sản phụ khoa, Hậu cần, Khác.");

            try {
                var aiResponse = await _geminiService.GetStaffSuggestionAsync(prompt.ToString());
                // Làm sạch response (Gemini đôi khi bọc trong ```json ... ```)
                var jsonStart = aiResponse.IndexOf('[');
                var jsonEnd = aiResponse.LastIndexOf(']');
                if (jsonStart >= 0 && jsonEnd > jsonStart) {
                    aiResponse = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }
                
                return Ok(aiResponse);
            } catch (Exception ex) {
                return BadRequest("Lỗi khi gọi AI: " + ex.Message);
            }
        }

        // TEST: api/MedicalGroups/{id}/ai-suggest-staff (GET)
        [HttpGet("{id}/ai-suggest-staff")]
        [AllowAnonymous] // Cho phép test trực tiếp bằng browser
        public IActionResult TestAiRoute(int id)
        {
            return Ok(new { 
                message = "Kết nối Backend OK! Route AI đã sẵn sàng.", 
                groupId = id,
                timestamp = DateTime.Now 
            });
        }

        // =========================================================================
        // PHẦN QUẢN LÝ VỊ TRÍ (POSITIONS)
        // =========================================================================

        // GET: api/MedicalGroups/{id}/positions
        [HttpGet("{id}/positions")]
        [AuthorizePermission("DoanKham.SetPosition")]
        public async Task<ActionResult<IEnumerable<MedicalGroupPositionDto>>> GetGroupPositions(int id)
        {
            return await _context.MedicalGroupPositions
                .Where(p => p.GroupId == id)
                .OrderBy(p => p.SortOrder)
                .Select(p => new MedicalGroupPositionDto
                {
                    PositionId = p.PositionId,
                    GroupId = p.GroupId,
                    PositionName = p.PositionName,
                    RequiredCount = p.RequiredCount,
                    AssignedCount = p.AssignedCount,
                    Description = p.Description,
                    SortOrder = p.SortOrder
                })
                .ToListAsync();
        }

        // POST: api/MedicalGroups/{id}/positions
        [HttpPost("{id}/positions")]
        [AuthorizePermission("DoanKham.SetPosition")]
        public async Task<IActionResult> AddPosition(int id, [FromBody] MedicalGroupPositionDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound("Đoàn khám không tồn tại.");

            var position = new MedicalGroupPosition
            {
                GroupId = id,
                PositionName = dto.PositionName,
                RequiredCount = dto.RequiredCount > 0 ? dto.RequiredCount : 1,
                Description = dto.Description,
                SortOrder = dto.SortOrder
            };

            _context.MedicalGroupPositions.Add(position);
            await _context.SaveChangesAsync();

            return Ok(position);
        }

        // DELETE: api/MedicalGroups/positions/{positionId}
        [HttpDelete("positions/{positionId}")]
        [AuthorizePermission("DoanKham.SetPosition")]
        public async Task<IActionResult> RemovePosition(int positionId)
        {
            var position = await _context.MedicalGroupPositions.FindAsync(positionId);
            if (position == null) return NotFound();

            var hasAssignedStaff = await _context.GroupStaffDetails.AnyAsync(gs => gs.PositionId == positionId);
            if (hasAssignedStaff)
                return BadRequest("Không thể xóa vị trí đã có nhân sự được phân công.");

            _context.MedicalGroupPositions.Remove(position);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
