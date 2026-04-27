using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services.MedicalGroups;
using System.Text;
using System.Text.RegularExpressions;

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
        private readonly IGroupLifecycleService _lifecycleService;
        private readonly Services.QrService _qrService;
        private readonly Services.Exports.IExportService _exportService;

        public MedicalGroupsController(
            ApplicationDbContext context,
            Services.IGeminiService geminiService,
            IMedicalGroupAutoAssignmentService autoAssignmentService,
            IGroupLifecycleService lifecycleService,
            Services.QrService qrService,
            Services.Exports.IExportService exportService)
        {
            _context = context;
            _geminiService = geminiService;
            _autoAssignmentService = autoAssignmentService;
            _lifecycleService = lifecycleService;
            _qrService = qrService;
            _exportService = exportService;
        }

        // GET: api/MedicalGroups
        [HttpGet]
        [AuthorizePermission("DoanKham.View")]
        public async Task<ActionResult<IEnumerable<MedicalGroupDto>>> GetMedicalGroups()
        {
            var today = DateTime.Today;
            return await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c!.Company)
                .OrderByDescending(g => g.GroupId)
                .Select(g => new MedicalGroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    ExamDate = g.ExamDate,
                    HealthContractId = g.HealthContractId,
                    ShortName = g.HealthContract != null && g.HealthContract.Company != null ? g.HealthContract.Company.ShortName : null,
                    CompanyName = g.HealthContract != null && g.HealthContract.Company != null ? g.HealthContract.Company.CompanyName : null,
                    Status = g.Status,
                    StartTime = g.StartTime,
                    DepartureTime = g.DepartureTime,
                    ExamContent = g.ExamContent,
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
            
            // Chặn tạo đoàn khám cho hợp đồng đã kết thúc hoặc bị khóa
            if (contract.Status == ContractStatus.Finished || contract.Status == ContractStatus.Locked || contract.Status == ContractStatus.Rejected)
                return BadRequest($"Hợp đồng đang ở trạng thái '{contract.Status}', không thể tạo thêm đoàn khám.");

            if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Approved) 
                return BadRequest("Hợp đồng chưa được phê duyệt. Vui lòng duyệt hợp đồng trước khi tạo đoàn khám.");

            var entity = new MedicalGroup
            {
                GroupName = dto.GroupName,
                ExamDate = dto.ExamDate,
                HealthContractId = dto.HealthContractId,
                Status = "Open",
                StartTime = dto.StartTime,
                DepartureTime = dto.DepartureTime,
                ExamContent = dto.ExamContent ?? "TỔNG QUÁT",
                ImportFilePath = dto.ImportFilePath,
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "system"
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
            group.Status = dto.Status ?? group.Status;
            group.StartTime = dto.StartTime;
            group.DepartureTime = dto.DepartureTime;
            group.ExamContent = dto.ExamContent;
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
            if (contract.Status != ContractStatus.Approved && contract.Status != ContractStatus.Active)
                return BadRequest("Hợp đồng chưa được duyệt hoặc chưa kích hoạt.");

            var start = contract.StartDate.Date;
            var end = contract.EndDate.Date;
            
            // Get all patients assigned to this contract
            var patients = await _context.Patients
                .Where(p => p.HealthContractId == contractId)
                .ToListAsync();

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var exists = await _context.MedicalGroups.AnyAsync(g =>
                    g.HealthContractId == contractId && g.ExamDate.Date == d);
                if (exists) continue;

                var newGroup = new MedicalGroup
                {
                    GroupName = $"{contract.Company?.ShortName ?? contract.Company?.CompanyName ?? "N/A"} - {d:dd/MM/yyyy}",
                    ExamDate = d,
                    HealthContractId = contractId,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "system"
                };
                
                _context.MedicalGroups.Add(newGroup);
                await _context.SaveChangesAsync(); // Save to generate GroupId

                // Move all Patients into MedicalRecords for this Group
                foreach (var patient in patients)
                {
                    var record = new MedicalRecord
                    {
                        GroupId = newGroup.GroupId,
                        PatientId = patient.PatientId,
                        FullName = patient.FullName,
                        DateOfBirth = patient.DateOfBirth,
                        Gender = patient.Gender,
                        IDCardNumber = patient.IDCardNumber,
                        Department = patient.Department,
                        Status = "READY",
                        QrToken = Guid.NewGuid().ToString("N"), // unique identifier if patient wants personal QR
                        CreatedAt = DateTime.Now
                    };
                    _context.MedicalRecords.Add(record);
                }
                if (patients.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { message = "Đã sinh đoàn theo dải ngày hợp đồng và chuyển danh sách bệnh nhân vào đoàn." });
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

        // GET: api/MedicalGroups/today-assignment
        [HttpGet("today-assignment")]
        [Authorize]
        public async Task<IActionResult> GetTodayAssignment()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode != null && s.EmployeeCode.ToLower() == username.ToLower());
            if (staff == null) return NotFound(new { message = "Không tìm thấy hồ sơ nhân sự." });

            var today = DateTime.Today;
            var assignment = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                    .ThenInclude(g => g!.HealthContract)
                        .ThenInclude(c => c!.Company)
                .Where(gsd => gsd.StaffId == staff.StaffId && gsd.MedicalGroup!.ExamDate.Date == today)
                .Select(gsd => new
                {
                    gsd.Id,
                    GroupId = gsd.MedicalGroup!.GroupId,
                    GroupName = gsd.MedicalGroup!.GroupName,
                    ExamDate = gsd.MedicalGroup!.ExamDate,
                    CompanyName = (gsd.MedicalGroup != null && gsd.MedicalGroup.HealthContract != null && gsd.MedicalGroup.HealthContract.Company != null) 
                        ? (gsd.MedicalGroup.HealthContract.Company.ShortName ?? gsd.MedicalGroup.HealthContract.Company.CompanyName) 
                        : null,
                    gsd.WorkPosition,
                    gsd.WorkStatus,
                    gsd.ShiftType,
                    gsd.CheckInTime,
                    gsd.CheckOutTime,
                    GroupStatus = gsd.MedicalGroup!.Status
                })
                .FirstOrDefaultAsync();

            if (assignment == null) return Ok(null);

            return Ok(assignment);
        }

        // GET: api/MedicalGroups/my-schedule — MedicalStaff xem lịch đi đoàn cá nhân
        [HttpGet("my-schedule")]
        [AuthorizePermission("LichKham.ViewOwn")]
        public async Task<IActionResult> GetMySchedule()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            // Tìm Staff tương ứng với tài khoản đang đăng nhập
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode != null && s.EmployeeCode.ToLower() == username.ToLower());
            if (staff == null) return NotFound(new { message = "Không tìm thấy hồ sơ nhân sự liên kết với tài khoản này." });

            var schedule = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                    .ThenInclude(g => g!.HealthContract)
                        .ThenInclude(c => c!.Company)
                .Where(gsd => gsd.StaffId == staff.StaffId)
                .OrderByDescending(gsd => gsd.MedicalGroup.ExamDate)
                .Select(gsd => new
                {
                    gsd.Id,
                    GroupId = gsd.MedicalGroup!.GroupId,
                    GroupName = gsd.MedicalGroup!.GroupName,
                    ExamDate = gsd.MedicalGroup!.ExamDate,
                    CompanyName = (gsd.MedicalGroup != null && gsd.MedicalGroup.HealthContract != null && gsd.MedicalGroup.HealthContract.Company != null) 
                        ? (gsd.MedicalGroup.HealthContract.Company.ShortName ?? gsd.MedicalGroup.HealthContract.Company.CompanyName) 
                        : null,
                    gsd.WorkPosition,
                    gsd.WorkStatus,
                    gsd.ShiftType,
                    gsd.CheckInTime,
                    gsd.CheckOutTime,
                    GroupStatus = gsd.MedicalGroup!.Status
                })
                .ToListAsync();

            return Ok(schedule);
        }

        // GET: api/MedicalGroups/active-today
        [HttpGet("active-today")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetActiveToday()
        {
            var today = DateTime.Today;
            var groups = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c!.Company)
                .Where(g => g.ExamDate.Date == today && g.Status == "Open")
                .Select(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    g.ExamDate,
                    CompanyName = g.HealthContract != null && g.HealthContract.Company != null ? g.HealthContract.Company.ShortName : null,
                    g.Status
                })
                .ToListAsync();
            return Ok(groups);
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
                    EmployeeCode = gsd.Staff != null ? gsd.Staff.EmployeeCode : null,
                    FullName = gsd.Staff != null ? gsd.Staff.FullName : "N/A",
                    JobTitle = gsd.Staff != null ? gsd.Staff.JobTitle : null,
                    ShiftType = gsd.ShiftType,
                    CalculatedSalary = gsd.CalculatedSalary,
                    WorkPosition = gsd.WorkPosition,
                    PositionId = gsd.PositionId,
                    WorkStatus = gsd.WorkStatus,
                    PickupLocation = gsd.PickupLocation,
                    Note = gsd.Note,
                    BirthYear = gsd.Staff != null ? gsd.Staff.BirthYear : null,
                    PhoneNumber = gsd.Staff != null ? gsd.Staff.PhoneNumber : null,
                    EmployeeType = gsd.Staff != null ? gsd.Staff.EmployeeType : null
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



            // Kiểm tra trùng lịch theo ngày + ca (ShiftType)
            var examDate = group.ExamDate.Date;
            var isOverlapping = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                .AnyAsync(gsd => gsd.StaffId == dto.StaffId
                    && gsd.MedicalGroup!.ExamDate.Date == examDate
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
                WorkStatus = dto.WorkStatus ?? "Đang chờ",
                PickupLocation = dto.PickupLocation,
                AssignedByUserId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null,
                AssignedAt = DateTime.UtcNow
            };



            _context.GroupStaffDetails.Add(detail);
            await _context.SaveChangesAsync();

            // TẠO THÔNG BÁO NỘI BỘ
            try
            {
                var userAccount = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == (staff.EmployeeCode != null ? staff.EmployeeCode.ToLower() : ""));
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
            if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Approved) return BadRequest("Hợp đồng chưa được phê duyệt.");

            var existingGroupsCount = await _context.MedicalGroups.CountAsync(g => g.HealthContractId == contractId);
            
            var newGroup = new MedicalGroup
            {
                GroupName = $"Đoàn khám {contract.Company?.ShortName ?? contract.Company?.CompanyName ?? "N/A"} - Lần {existingGroupsCount + 1}",
                ExamDate = DateTime.Now.AddDays(7), // Mặc định 7 ngày sau
                HealthContractId = contractId,
                Status = "Open",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "system"
            };

            _context.MedicalGroups.Add(newGroup);
            await _context.SaveChangesAsync(); // generate GroupId

            var patients = await _context.Patients
                .Where(p => p.HealthContractId == contractId)
                .ToListAsync();

            if (patients.Any())
            {
                foreach (var patient in patients)
                {
                    var record = new MedicalRecord
                    {
                        GroupId = newGroup.GroupId,
                        PatientId = patient.PatientId,
                        FullName = patient.FullName,
                        DateOfBirth = patient.DateOfBirth,
                        Gender = patient.Gender,
                        IDCardNumber = patient.IDCardNumber,
                        Department = patient.Department,
                        Status = "READY",
                        QrToken = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now
                    };
                    _context.MedicalRecords.Add(record);
                }
                await _context.SaveChangesAsync();
            }

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
                var myStaff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode != null && s.EmployeeCode.ToLower() == (username != null ? username.ToLower() : ""));
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
                    OldValue = "",
                    NewValue = $"Staff {detail.Staff?.FullName ?? "N/A"} check-in lúc {detail.CheckInTime:HH:mm}",
                    Timestamp = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
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
                var myStaff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode != null && s.EmployeeCode.ToLower() == (username != null ? username.ToLower() : ""));
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
                    OldValue = "",
                    NewValue = $"Staff {detail.Staff?.FullName ?? "N/A"} check-out lúc {detail.CheckOutTime:HH:mm}",
                    Timestamp = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
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
                .ThenInclude(c => c!.Company)
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
                    worksheet.Cell(i + 2, 2).Value = groups[i].HealthContract?.Company?.ShortName ?? groups[i].HealthContract?.Company?.CompanyName ?? "N/A";
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
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c!.Company)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null) return NotFound();

            var staff = await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.GroupId == id)
                .OrderBy(gsd => gsd.PositionId)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Phân công nhân sự");
                
                // --- 1. HEADER TITLE ---
                var titleCell = worksheet.Cell(1, 1);
                titleCell.Value = "DANH SÁCH NHÂN SỰ ĐI ĐOÀN KHÁM SỨC KHỎE";
                worksheet.Range(1, 1, 1, 10).Merge().Style
                    .Font.SetBold(true)
                    .Font.SetFontSize(16)
                    .Font.SetFontColor(ClosedXML.Excel.XLColor.White)
                    .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.FromHtml("#1e40af")); // Màu xanh Blue chuyên nghiệp
                worksheet.Row(1).Height = 40;

                // --- 2. ADMINISTRATIVE INFO ---
                worksheet.Cell(3, 1).Value = "Đoàn khám:";
                worksheet.Cell(3, 2).Value = group.GroupName;
                worksheet.Range(3, 2, 3, 4).Merge().Style.Font.SetBold(true);

                worksheet.Cell(3, 6).Value = "Ngày khám:";
                worksheet.Cell(3, 7).Value = group.ExamDate.ToString("dd/MM/yyyy");
                worksheet.Cell(3, 7).Style.Font.SetBold(true);

                worksheet.Cell(4, 1).Value = "Nội dung:";
                worksheet.Cell(4, 2).Value = group.ExamContent ?? "TỔNG QUÁT";
                worksheet.Range(4, 2, 4, 4).Merge().Style.Font.SetBold(true);

                worksheet.Cell(4, 6).Value = "Giờ khám:";
                worksheet.Cell(4, 7).Value = group.StartTime ?? "---";
                worksheet.Cell(4, 7).Style.Font.SetBold(true);

                worksheet.Cell(5, 1).Value = "Công ty:";
                worksheet.Cell(5, 2).Value = group.HealthContract?.Company?.CompanyName ?? "---";
                worksheet.Range(5, 2, 5, 4).Merge();

                worksheet.Cell(5, 6).Value = "Giờ xuất phát:";
                worksheet.Cell(5, 7).Value = group.DepartureTime ?? "---";
                worksheet.Cell(5, 7).Style.Font.SetBold(true).Font.SetFontColor(ClosedXML.Excel.XLColor.Red);

                // --- 3. TABLE HEADERS ---
                string[] headers = { "STT", "HỌ VÀ TÊN", "CÔNG VIỆC (VỊ TRÍ)", "NĂM SINH", "CHỨC DANH", "ĐIỆN THOẠI", "ĐƠN VỊ", "GHI CHÚ", "ĐÓN TẠI", "KÝ TÊN" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(7, i + 1);
                    cell.Value = headers[i];
                    cell.Style
                        .Font.SetBold(true)
                        .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center)
                        .Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.FromHtml("#f1f5f9"))
                        .Border.SetOutsideBorder(ClosedXML.Excel.XLBorderStyleValues.Thin);
                }
                worksheet.Row(7).Height = 25;

                // --- 4. DATA ROWS ---
                int startRow = 8;
                for (int i = 0; i < staff.Count; i++)
                {
                    var s = staff[i];
                    worksheet.Cell(startRow + i, 1).Value = i + 1;
                    worksheet.Cell(startRow + i, 2).Value = s.Staff?.FullName;
                    worksheet.Cell(startRow + i, 3).Value = s.WorkPosition;
                    worksheet.Cell(startRow + i, 4).Value = s.Staff?.BirthYear;
                    worksheet.Cell(startRow + i, 5).Value = s.Staff?.JobTitle;
                    worksheet.Cell(startRow + i, 6).Value = s.Staff?.PhoneNumber;
                    worksheet.Cell(startRow + i, 7).Value = (s.Staff?.EmployeeType == "Internal" || s.Staff?.EmployeeType == "NoiBo") ? "NỘI BỘ" : "THUÊ NGOÀI";
                    worksheet.Cell(startRow + i, 8).Value = s.Note;
                    worksheet.Cell(startRow + i, 9).Value = s.PickupLocation;
                    worksheet.Cell(startRow + i, 10).Value = ""; // Ký tên

                    // Style row
                    var rowRange = worksheet.Range(startRow + i, 1, startRow + i, 10);
                    rowRange.Style.Border.SetOutsideBorder(ClosedXML.Excel.XLBorderStyleValues.Thin);
                    rowRange.Style.Alignment.SetVertical(ClosedXML.Excel.XLAlignmentVerticalValues.Center);
                    worksheet.Cell(startRow + i, 1).Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);
                    worksheet.Cell(startRow + i, 4).Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center); // Năm sinh
                }

                // --- 5. FOOTER & SIGNATURE ---
                int footerRow = startRow + staff.Count + 2;
                var dateRange = worksheet.Range(footerRow, 8, footerRow, 10);
                dateRange.Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                dateRange.Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center).Font.SetItalic(true);
                
                var signLabelRange = worksheet.Range(footerRow + 1, 8, footerRow + 1, 10);
                signLabelRange.Merge().Value = "NGƯỜI LẬP BẢNG";
                signLabelRange.Style.Font.SetBold(true).Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);
                
                var userNameRange = worksheet.Range(footerRow + 5, 8, footerRow + 5, 10);
                userNameRange.Merge().Value = User.Identity?.Name ?? "Quản trị viên";
                userNameRange.Style.Font.SetBold(true).Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);

                // Auto adjust columns
                worksheet.Columns().AdjustToContents();
                worksheet.Column(1).Width = 5; // STT
                worksheet.Column(2).Width = 30; // Họ tên
                worksheet.Column(3).Width = 20; // Vị trí
                worksheet.Column(10).Width = 15; // Ký tên

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PhanCong_{group.GroupName}.xlsx");
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
                .ThenInclude(c => c!.Company)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null) return NotFound("Đoàn khám không tồn tại.");

            // 1. Thu thập dữ liệu ngữ cảnh
            var contract = group.HealthContract;
            var expectedPeople = contract?.ExpectedQuantity ?? 0;
            
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

            prompt.AppendLine("\nYêu cầu phân bộ (CLINICAL RULES - QUAN TRỌNG):");
            prompt.AppendLine("1. QUY TẮC BÁC SĨ (MUST): Bác sĩ (BacSi) CHỈ được phân vào các vị trí khám chuyên khoa (Khám nội, Khám ngoại, Siêu âm, Khám sản phụ khoa, Tai mũi họng). CẤM bác sĩ làm Tiếp nhận, Hậu cần hoặc Lấy máu.");
            prompt.AppendLine("2. QUY TẮC ĐIỀU DƯỠNG (PREFER): DieuDuong hoặc KyThuatVien ưu tiên cho Lấy máu, Cân đo huyết áp, Tiếp nhận.");
            prompt.AppendLine("3. Định mức: Tổng số nhân sự khoảng 1:15 đến 1:20 so với quy mô người khám. Nếu quy mô nhỏ, hãy ưu tiên chất lượng (đủ các vị trí khám).");
            prompt.AppendLine("4. AI thông minh: Nếu một nhân sự có Specialty (Chuyên khoa) khớp với tên vị trí (ví dụ: Specialty 'SieuAm' cho vị trí 'Siêu âm'), hãy ƯU TIÊN TUYỆT ĐỐI nhân sự đó.");
            prompt.AppendLine("5. Tránh lãng phí: Đừng gán quá nhiều người vào một vị trí nếu quy mô đoàn nhỏ.");
            prompt.AppendLine("\nKết quả trả về DUY NHẤT một mảng JSON format: [{\"staffId\": int, \"staffName\": string, \"workPosition\": string, \"shiftType\": float, \"reason\": string}]");
            prompt.AppendLine("Vị trí (workPosition) phải là một trong các vị trí có trong đoàn hoặc dựa trên các vị trí phổ biến: Tiếp nhận, Cân đo huyết áp, Khám nội, Khám ngoại, Lấy máu, Siêu âm, Khám sản phụ khoa, Hậu cần, Tai mũi họng.");

            try {
                var aiResponse = await _geminiService.GetStaffSuggestionAsync(prompt.ToString());
                
                // Loại bỏ Markdown Wrapper nếu có (AI đôi khi vẫn trả về ```json ... ``` dù đã set mimeType)
                if (aiResponse.StartsWith("```json"))
                {
                    aiResponse = aiResponse.Substring(7);
                    if (aiResponse.EndsWith("```"))
                    {
                        aiResponse = aiResponse.Substring(0, aiResponse.Length - 3);
                    }
                }
                else if (aiResponse.StartsWith("```"))
                {
                    aiResponse = aiResponse.Substring(3);
                    if (aiResponse.EndsWith("```"))
                    {
                        aiResponse = aiResponse.Substring(0, aiResponse.Length - 3);
                    }
                }
                
                aiResponse = aiResponse.Trim();

                // Kiểm tra xem parse có thành công không
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse);
                
                // Return original JSON safely as structure
                return Content(aiResponse, "application/json");
            } catch (System.Text.Json.JsonException jsonEx) {
                // Log lỗi chi tiết ra console
                Console.WriteLine($"[AI_JSON_ERROR] {DateTime.Now}: {jsonEx.Message}");
                return BadRequest("AI trả về chuỗi JSON không hợp lệ. Vui lòng thử lại.");
            } catch (Exception ex) {
                Console.WriteLine($"[AI_ERROR] {DateTime.Now}: {ex.Message}");
                return BadRequest("Lỗi khi phân tích Dữ liệu AI: " + ex.Message);
            }
        }


        // GET: api/MedicalGroups/{id}/lock-status
        // Kiểm tra điều kiện và tự đánh dấu ReadyToLock nếu đủ điều kiện
        [HttpGet("{id}/lock-status")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetLockStatus(int id)
        {
            var result = await _lifecycleService.CheckReadyToLockAsync(id);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        // POST: api/MedicalGroups/{id}/lock
        // Admin xác nhận khóa sổ — tính lương, tạo CostSnapshot, chuyển status = Locked
        [HttpPost("{id}/lock")]
        [AuthorizePermission("DoanKham.Lock")]
        public async Task<IActionResult> LockGroup(int id)
        {
            var actorUserId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            var result = await _lifecycleService.LockGroupAsync(id, actorUserId);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        // POST: api/MedicalGroups/admin/backfill-lock
        // One-time migration: đánh dấu Locked cho các group cũ đang ở status Finished
        [HttpPost("admin/backfill-lock")]
        [AuthorizePermission("DoanKham.Lock")]
        public async Task<IActionResult> BackfillLock()
        {
            var actorUserId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            var result = await _lifecycleService.BackfillFinishedGroupsAsync(actorUserId);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
        // GET: api/MedicalGroups/{id}/positions
        [HttpGet("{id}/positions")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<ActionResult<IEnumerable<MedicalGroupPositionDto>>> GetGroupPositions(int id)
        {
            return await _context.MedicalGroupPositions
                .Where(p => p.GroupId == id)
                .OrderBy(p => p.SortOrder)
                .Select(p => new MedicalGroupPositionDto
                {
                    PositionId = p.Id,
                    GroupId = p.GroupId,
                    PositionName = p.PositionName,
                    RequiredCount = p.RequiredCount,
                    AssignedCount = _context.GroupStaffDetails.Count(gsd => gsd.GroupId == id && gsd.PositionId == p.PositionId),
                    Description = p.Description,
                    SortOrder = p.SortOrder
                })
                .ToListAsync();
        }

        // POST: api/MedicalGroups/{id}/positions
        [HttpPost("{id}/positions")]
        [AuthorizePermission("DoanKham.SetPosition")]
        public async Task<ActionResult<MedicalGroupPosition>> PostPosition(int id, MedicalGroupPositionDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound("Đoàn khám không tồn tại");

            // Cố gắng tìm Position chuẩn tương ứng để link (nếu có)
            var basePositionId = await _context.Positions
                .Where(p => p.Name.ToLower() == dto.PositionName.ToLower() || p.Code.ToLower() == dto.PositionName.ToLower())
                .Select(p => (int?)p.PositionId)
                .FirstOrDefaultAsync();

            var entity = new MedicalGroupPosition
            {
                GroupId = id,
                PositionId = basePositionId,
                PositionName = dto.PositionName,
                RequiredCount = dto.RequiredCount,
                Description = dto.Description ?? "",
                SortOrder = dto.SortOrder
            };

            _context.MedicalGroupPositions.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        // DELETE: api/MedicalGroups/positions/{id}
        [HttpDelete("positions/{id}")]
        [AuthorizePermission("DoanKham.SetPosition")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            var position = await _context.MedicalGroupPositions.FindAsync(id);
            if (position == null) return NotFound();

            // Check if any staff assigned to this position
            var hasStaff = await _context.GroupStaffDetails.AnyAsync(gsd => gsd.PositionId == id);
            if (hasStaff) return BadRequest("Không thể xóa vị trí đã có nhân sự phân công.");

            _context.MedicalGroupPositions.Remove(position);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/MedicalGroups/{id}/supplies-report
        [HttpGet("{id}/supplies-report")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetSuppliesReport(int id)
        {
            var groupExists = await _context.MedicalGroups.AnyAsync(g => g.GroupId == id);
            if (!groupExists) return NotFound("Không tìm thấy đoàn khám");

            var totalPatients = await _context.MedicalRecords.CountAsync(m => m.GroupId == id);
            var expectedPatients = totalPatients == 0 ? 1 : totalPatients; // Prevent Div/0

            var activeMovements = await _context.StockMovements
                .Where(m => m.MedicalGroupId == id && m.MovementType == "OUT")
                .GroupBy(m => new { m.ItemName, m.Unit })
                .Select(g => new
                {
                    ItemName = g.Key.ItemName,
                    Unit = g.Key.Unit,
                    TotalUsed = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var result = activeMovements.Select(m => {
                var expected = Math.Ceiling(expectedPatients * 1.1); // 10% variance allowed
                var percentage = Math.Round((double)m.TotalUsed / expected * 100, 1);
                var status = percentage > 120 ? "CRITICAL" : (percentage > 100 ? "WARNING" : "NORMAL");

                return new {
                    m.ItemName,
                    m.Unit,
                    m.TotalUsed,
                    ExpectedUsage = expected,
                    UsagePercentage = percentage,
                    Status = status
                };
            });

            return Ok(new
            {
                GroupId = id,
                TotalPatients = totalPatients,
                Supplies = result
            });
        }

        // GET: api/MedicalGroups/{id}/ai-suggest-staff
        [HttpGet("{id}/ai-suggest-staff")]
        [AuthorizePermission("AI.SuggestStaff")]
        public async Task<IActionResult> GetAiStaffSuggestions(int id, [FromQuery] string? positionName)
        {
            var suggestionService = new StaffSuggestionService(_context);

            List<StaffSuggestionResult> suggestions;
            if (!string.IsNullOrEmpty(positionName))
            {
                suggestions = await suggestionService.SuggestStaffForPositionAsync(id, positionName);
            }
            else
            {
                suggestions = await suggestionService.SuggestStaffForGroupAsync(id);
            }

            return Ok(new
            {
                groupId = id,
                positionName,
                totalSuggestions = suggestions.Count,
                suggestions = suggestions.Select(s => new
                {
                    s.StaffId,
                    s.StaffName,
                    s.StaffType,
                    s.JobTitle,
                    s.PositionName,
                    s.Score,
                    s.Reason
                })
            });
        }

        // POST: api/MedicalGroups/{id}/apply-suggestion
        [HttpPost("{id}/apply-suggestion")]
        [AuthorizePermission("DoanKham.AssignStaff")]
        public async Task<IActionResult> ApplyStaffSuggestion(int id, [FromBody] ApplySuggestionRequest request)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound("Không tìm thấy đoàn khám");

            var staff = await _context.Staffs.FindAsync(request.StaffId);
            if (staff == null) return NotFound("Không tìm thấy nhân sự");

            // Kiểm tra nhân sự đã được phân công vào đoàn khám này chưa
            var existingAssignment = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(gsd =>
                    gsd.GroupId == id &&
                    gsd.StaffId == request.StaffId &&
                    gsd.ExamDate.Date == group.ExamDate.Date);

            if (existingAssignment != null)
                return BadRequest(new { message = "Nhân sự đã được phân công vào đoàn khám này" });

            // Kiểm tra nhân sự đã được phân công vào ngày khác chưa
            var otherAssignment = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(gsd =>
                    gsd.StaffId == request.StaffId &&
                    gsd.ExamDate.Date == group.ExamDate.Date &&
                    gsd.WorkStatus != "Absent");

            if (otherAssignment != null)
            {
                var otherGroup = await _context.MedicalGroups.FindAsync(otherAssignment.GroupId);
                return BadRequest(new
                {
                    message = $"Nhân sự đã được phân công vào đoàn khám khác trong ngày này: {otherGroup?.GroupName}"
                });
            }

            // Tạo phân công mới
            var assignment = new GroupStaffDetail
            {
                GroupId = id,
                StaffId = request.StaffId,
                WorkPosition = request.PositionName,
                ExamDate = group.ExamDate,
                WorkStatus = "Pending",
                AssignedAt = DateTime.Now,
                AssignedByUserId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null
            };

            _context.GroupStaffDetails.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã áp dụng gợi ý phân công",
                assignmentId = assignment.Id,
                staffId = request.StaffId,
                staffName = staff.FullName,
                positionName = request.PositionName
            });
        }

        // GET: api/MedicalGroups/{id}/patients
        [HttpGet("{id}/patients")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetGroupPatients(int id)
        {
            var patients = await _context.MedicalRecords
                .Include(mr => mr.Patient)
                .Where(mr => mr.GroupId == id)
                .Select(mr => new
                {
                    mr.PatientId,
                    mr.FullName,
                    mr.DateOfBirth,
                    mr.Gender,
                    PhoneNumber = mr.Patient != null ? mr.Patient.PhoneNumber : "",
                    mr.Department,
                    ExamFunction = "",
                    Notes = "",
                    AddedAt = mr.CreatedAt
                })
                .ToListAsync();

            return Ok(patients);
        }

        // POST: api/MedicalGroups/{id}/patients/import
        [HttpPost("{id}/patients/import")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> ImportPatients(int id, [FromBody] List<PatientBatchImportDto> patients)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound("Không tìm thấy đoàn khám");

            var userId = User.FindFirst("UserId")?.Value ?? "system";
            var importBatchId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var importedCount = 0;
            var errors = new List<string>();

            foreach (var patientDto in patients)
            {
                try
                {
                    // Kiểm tra trùng lặp theo IDCardNumber
                    if (!string.IsNullOrEmpty(patientDto.IDCardNumber))
                    {
                        var existingPatient = await _context.Patients
                            .FirstOrDefaultAsync(p => p.IDCardNumber == patientDto.IDCardNumber);

                        Patient patient;
                        if (existingPatient != null)
                        {
                            // Cập nhật thông tin nếu đã tồn tại
                            existingPatient.FullName = patientDto.FullName;
                            existingPatient.PhoneNumber = patientDto.PhoneNumber;
                            existingPatient.Department = patientDto.Department;
                            patient = existingPatient;
                        }
                        else
                        {
                            // Tạo bệnh nhân mới
                            patient = new Patient
                            {
                                FullName = patientDto.FullName,
                                DateOfBirth = patientDto.DateOfBirth ?? DateTime.Now.AddYears(-30),
                                Gender = patientDto.Gender,
                                IDCardNumber = patientDto.IDCardNumber,
                                PhoneNumber = patientDto.PhoneNumber,
                                Department = patientDto.Department,
                                HealthContractId = group.HealthContractId,
                                CreatedDate = DateTime.Now
                            };
                            _context.Patients.Add(patient);
                            await _context.SaveChangesAsync();
                        }

                        // Kiểm tra đã có trong đoàn khám này chưa
                        var existingInGroup = await _context.MedicalRecords
                            .AnyAsync(mr => mr.GroupId == id && mr.PatientId == patient.PatientId);

                        if (!existingInGroup)
                        {
                            // Thêm vào đoàn khám
                            var record = new MedicalRecord
                            {
                                GroupId = id,
                                PatientId = patient.PatientId,
                                FullName = patient.FullName,
                                DateOfBirth = patient.DateOfBirth,
                                Gender = patient.Gender,
                                IDCardNumber = patient.IDCardNumber,
                                Department = patient.Department,
                                QrToken = Guid.NewGuid().ToString("N"),
                                Status = "CREATED",
                                CreatedAt = DateTime.Now
                            };
                            _context.MedicalRecords.Add(record);
                            importedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Lỗi với bệnh nhân {patientDto.FullName}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã import {importedCount} bệnh nhân",
                importBatchId,
                importedCount,
                errors = errors.Count > 0 ? errors : null
            });
        }

        // DELETE: api/MedicalGroups/{id}/patients/{patientId}
        [HttpDelete("{id}/patients/{patientId}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> RemovePatientFromGroup(int id, int patientId)
        {
            var record = await _context.MedicalRecords
                .FirstOrDefaultAsync(mr => mr.GroupId == id && mr.PatientId == patientId);

            if (record == null) return NotFound("Bệnh nhân không có trong đoàn khám này");

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa bệnh nhân khỏi đoàn khám" });
        }

        // ================================================================
        // GET: api/MedicalGroups/{id}/export-patients — Export DS bệnh nhân
        // ================================================================
        [HttpGet("{id}/export-patients")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportPatients(int id)
        {
            try
            {
                var bytes = await _exportService.ExportPatientsByGroupAsync(id);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DSCN_Doan{id}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================================================================
        // GET: api/MedicalGroups/{id}/export-pnl — Export P&L đoàn khám
        // ================================================================
        [HttpGet("{id}/export-pnl")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportPnl(int id)
        {
            try
            {
                var bytes = await _exportService.ExportGroupPnlAsync(id);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"PNL_Doan{id}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class ApplySuggestionRequest
    {
        public int StaffId { get; set; }
        public string PositionName { get; set; } = null!;
    }

    public class PatientBatchImportDto
    {
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IDCardNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? ExamFunction { get; set; }
        public string? Notes { get; set; }
    }
}
