using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>
    /// Controller quản lý chấm công theo QR code và tính ngày công.
    /// Luồng: Trưởng đoàn mở QR → nhân viên quét → check-in/out.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AttendanceController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ================================================================
        // GET api/attendance/qr/{groupId} — Tạo QR token cho trưởng đoàn
        // ================================================================
        [HttpGet("qr/{groupId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.QR")]
        public async Task<IActionResult> GenerateQrToken(int groupId, [FromServices] Services.QrService qrService)
        {
            var group = await _context.MedicalGroups.FindAsync(groupId);
            if (group == null) return NotFound("Không tìm thấy đoàn khám.");
            if (group.Status == "Locked" || group.Status == "Finished")
                return BadRequest("Đoàn khám đã kết thúc, không thể mở QR chấm công.");

            // Sử dụng QrService mới để tạo Token có chữ ký bảo mật
            var token = qrService.GenerateSignedToken(groupId, 12);

            // URL trỏ về Frontend (CheckIn.vue)
            var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var frontendUrl = $"{baseUrl}/checkin?token={Uri.EscapeDataString(token)}";
            
            var pngBase64 = qrService.GenerateQr(frontendUrl);

            return Ok(new
            {
                groupId,
                groupName = group.GroupName,
                examDate = group.ExamDate,
                qrToken = token,
                expiresAt = DateTime.Now.AddHours(12),
                qrUrl = frontendUrl,
                pngBase64 = pngBase64
            });
        }

        // ================================================================
        // GET api/attendance/active-qr-today — Lấy QR cho đoàn khám đang diễn ra
        // ================================================================
        [HttpGet("active-qr-today")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.QR")]
        public async Task<IActionResult> GetActiveQrToday([FromServices] Services.QrService qrService, [FromQuery] string? origin = null)
        {
            var today = DateTime.Today;
            // 1. Tìm đoàn khám sẵn sàng (Open hoặc InProgress) cho hôm nay
            var activeGroup = await _context.MedicalGroups
                .OrderBy(g => g.GroupId)
                .FirstOrDefaultAsync(g => g.ExamDate.Date == today && (g.Status == "Open" || g.Status == "InProgress"));

            if (activeGroup == null) 
            {
                // 2. Chẩn đoán lý do nếu không tìm thấy đoàn phù hợp
                var anyToday = await _context.MedicalGroups
                    .FirstOrDefaultAsync(g => g.ExamDate.Date == today);
                
                if (anyToday != null)
                {
                    return NotFound(new { 
                        message = $"Tìm thấy đoàn '{anyToday.GroupName}' cho hôm nay nhưng đang ở trạng thái '{anyToday.Status}'. Vui lòng chuyển trạng thái sang 'Open' để mở QR.",
                        status = anyToday.Status
                    });
                }

                var nextGroup = await _context.MedicalGroups
                    .Where(g => g.ExamDate.Date > today)
                    .OrderBy(g => g.ExamDate)
                    .FirstOrDefaultAsync();

                if (nextGroup != null)
                {
                    return NotFound(new { 
                        message = $"Hôm nay không có đoàn khám. Đoàn tiếp theo diễn ra vào ngày {nextGroup.ExamDate:dd/MM/yyyy} ({nextGroup.GroupName})."
                    });
                }

                return NotFound(new { message = "Hôm nay không có đoàn khám nào diễn ra." });
            }

            // Sinh token ngắn hạn (12h). Frontend sẽ lo việc làm mới link.
            var token = qrService.GenerateSignedToken(activeGroup.GroupId, 12);
            var baseUrl = !string.IsNullOrEmpty(origin) ? origin : (_configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173");
            var frontendUrl = $"{baseUrl}/checkin?token={Uri.EscapeDataString(token)}";
            var pngBase64 = qrService.GenerateQr(frontendUrl);

            return Ok(new
            {
                groupId = activeGroup.GroupId,
                groupName = activeGroup.GroupName,
                qrToken = token,
                qrUrl = frontendUrl,
                pngBase64 = pngBase64,
                expiresAt = DateTime.Now.AddHours(12)
            });
        }

        // ================================================================
        // GET api/attendance/patient-qr — Lấy QR cho link bệnh nhân tự báo danh
        // ================================================================
        [HttpGet("patient-qr")]
        [AllowAnonymous]
        public IActionResult GetPatientQr([FromServices] Services.QrService qrService, [FromQuery] string? origin = null)
        {
            var baseUrl = !string.IsNullOrEmpty(origin) ? origin : (_configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173");
            var frontendUrl = $"{baseUrl}/patient-checkin";
            var pngBase64 = qrService.GenerateQr(frontendUrl);

            return Ok(new
            {
                qrUrl = frontendUrl,
                pngBase64 = pngBase64
            });
        }

        // ================================================================
        // GET api/attendance/recent-checkins — Lấy 5 người vừa check-in để hiện trên Kiosk
        // ================================================================
        [HttpGet("recent-checkins")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.ViewAll")]
        public async Task<IActionResult> GetRecentCheckins()
        {
            var today = DateTime.Today;
            var recent = await _context.ScheduleCalendars
                .Include(sc => sc.Staff)
                .Include(sc => sc.MedicalGroup)
                .Where(sc => sc.ExamDate.Date == today && sc.CheckInTime != null)
                .OrderByDescending(sc => sc.CheckInTime)
                .Take(5)
                .Select(sc => new
                {
                    sc.CalendarId,
                    StaffName = sc.Staff != null ? sc.Staff.FullName : "N/A",
                    CheckInTime = sc.CheckInTime,
                    GroupName = sc.MedicalGroup != null ? sc.MedicalGroup.GroupName : "N/A"
                })
                .ToListAsync();

            return Ok(recent);
        }


        // ================================================================
        // POST api/attendance/self-checkin-direct — Một chạm (không cần QR)
        // ================================================================
        [HttpPost("self-checkin-direct")]
        [Authorize]
        public async Task<IActionResult> SelfCheckInDirect([FromBody] int groupId)
        {
            // 1. Xác định Staff từ User hiện tại
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode != null && s.EmployeeCode.ToLower() == username.ToLower());
            if (staff == null) return NotFound(new { message = "Không tìm thấy hồ sơ nhân sự liên kết với tài khoản này." });

            int staffId = staff.StaffId;

            // 2. Kiểm tra Đoàn khám
            var group = await _context.MedicalGroups.FindAsync(groupId);
            if (group == null) return NotFound(new { message = "Không tìm thấy đoàn khám." });

            // Chống gian lận: Chỉ cho phép nếu đoàn đang Open hoặc InProgress
            if (group.Status != "Open" && group.Status != "InProgress")
                return BadRequest(new { message = $"Đoàn khám đang ở trạng thái '{group.Status}', không thể điểm danh." });

            // Chỉ cho phép điểm danh đúng ngày
            var today = DateTime.Today;
            if (group.ExamDate.Date != today)
                return BadRequest(new { message = "Bạn chỉ có thể điểm danh cho đoàn khám diễn ra trong ngày hôm nay." });

            // Kiểm tra nhân viên có trong đoàn không
            var groupDetail = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(gd => gd.GroupId == groupId && gd.StaffId == staffId);
            if (groupDetail == null)
                return BadRequest(new { message = "Bạn không được phân công tham gia đoàn khám này." });

            // 3. Thực hiện ghi nhận Logic tương tự Check-in QR
            var existing = await _context.ScheduleCalendars
                .FirstOrDefaultAsync(sc => sc.GroupId == groupId && sc.StaffId == staffId
                    && sc.ExamDate.Date == today);

            string actionResult = "";
            DateTime now = DateTime.Now;

            if (existing == null)
            {
                // Check-in lần đầu
                _context.ScheduleCalendars.Add(new ScheduleCalendar
                {
                    GroupId = groupId,
                    StaffId = staffId,
                    ExamDate = today,
                    MedicalGroup = null!,
                    Staff = null!,
                    CheckInTime = now,
                    IsConfirmed = false,
                    Note = "Điểm danh trực tiếp qua Dashboard"
                });
                actionResult = "CheckIn";
            }
            else if (existing.CheckOutTime == null)
            {
                // Check-out
                existing.CheckOutTime = now;
                var hours = (existing.CheckOutTime.Value - existing.CheckInTime!.Value).TotalHours;
                var resolvedShift = hours >= 4 ? 1.0 : 0.5;
                groupDetail.ShiftType = resolvedShift;
                existing.IsConfirmed = true;
                actionResult = "CheckOut";
            }
            else
            {
                return BadRequest(new { message = "Bạn đã hoàn thành đủ công (Check-in & Check-out) cho đoàn này hôm nay." });
            }

            // 4. Ghi Audit Log cho hành động "Một chạm"
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (currentUser != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = currentUser.UserId,
                    Action = actionResult == "CheckIn" ? "SELF_CHECK_IN" : "SELF_CHECK_OUT",
                    EntityType = "GroupStaffDetail",
                    EntityId = groupDetail.Id,
                    OldValue = "",
                    NewValue = $"[{actionResult}] Staff {staff.FullName} thực hiện điểm danh MỘT CHẠM lúc {now:HH:mm}",
                    Timestamp = now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = actionResult == "CheckIn" ? "Xác nhận tham gia thành công!" : "Check-out thành công!", 
                action = actionResult, 
                time = now 
            });
        }

        // ================================================================
        // POST api/attendance/checkin — Nhân viên quét QR → check-in
        // ================================================================

        // ================================================================
        // POST api/attendance/manual — Trưởng đoàn chấm công thủ công
        // ================================================================
        [HttpPost("manual")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.CheckInOut")]
        public async Task<IActionResult> ManualCheckIn([FromBody] CheckInOutDto dto)
        {
            var groupDetail = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(gd => gd.GroupId == dto.GroupId && gd.StaffId == dto.StaffId);
            if (groupDetail == null) return NotFound("Nhân viên không thuộc đoàn này.");

            var today = DateTime.Today;
            var existing = await _context.ScheduleCalendars
                .FirstOrDefaultAsync(sc => sc.GroupId == dto.GroupId && sc.StaffId == dto.StaffId && sc.ExamDate.Date == today);

            if (existing == null)
            {
                _context.ScheduleCalendars.Add(new ScheduleCalendar
                {
                    GroupId = dto.GroupId,
                    StaffId = dto.StaffId,
                    ExamDate = today,
                    MedicalGroup = null!,
                    Staff = null!,
                    CheckInTime = DateTime.Now,
                    IsConfirmed = false,
                    Note = dto.Note ?? "Chấm công thủ công bởi trưởng đoàn"
                });
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã ghi nhận check-in thủ công." });
            }
            else if (existing.CheckOutTime == null)
            {
                existing.CheckOutTime = DateTime.Now;
                var hours = (existing.CheckOutTime.Value - existing.CheckInTime!.Value).TotalHours;
                groupDetail.ShiftType = hours >= 4 ? 1.0 : 0.5;
                existing.IsConfirmed = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã ghi nhận check-out thủ công.", shiftType = groupDetail.ShiftType });
            }
            else
            {
                return BadRequest("Nhân viên đã đủ công trong ngày này.");
            }
        }

        // ================================================================
        // GET api/attendance/summary/{staffId}?month=4&year=2026
        // ================================================================
        [HttpGet("summary/{staffId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.ViewAll")]
        public async Task<IActionResult> GetAttendanceSummary(int staffId, [FromQuery] int month, [FromQuery] int year)
        {
            if (month == 0) month = DateTime.Now.Month;
            if (year == 0) year = DateTime.Now.Year;

            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff == null) return NotFound();

            var details = await _context.ScheduleCalendars
                .Include(sc => sc.MedicalGroup)
                .Where(sc => sc.StaffId == staffId
                    && sc.ExamDate.Month == month && sc.ExamDate.Year == year)
                .ToListAsync();

            var groupDetails = await _context.GroupStaffDetails
                .Where(gd => gd.StaffId == staffId)
                .ToListAsync();

            var summaryDetails = details.Select(sc =>
            {
                var gd = groupDetails.FirstOrDefault(g => g.GroupId == sc.GroupId);
                return new AttendanceDetailDto
                {
                    GroupId = sc.GroupId,
                    GroupName = sc.MedicalGroup?.GroupName ?? $"Đoàn #{sc.GroupId}",
                    ExamDate = sc.ExamDate,
                    ShiftType = gd?.ShiftType ?? (sc.IsConfirmed ? 1.0 : 0),
                    CheckInTime = sc.CheckInTime,
                    CheckOutTime = sc.CheckOutTime,
                    WorkStatus = sc.IsConfirmed ? "Đủ công" : (sc.CheckInTime != null ? "Chờ check-out" : "Vắng")
                };
            }).ToList();

            var totalDays = summaryDetails.Sum(d => d.ShiftType);

            return Ok(new AttendanceSummaryDto
            {
                StaffId = staffId,
                StaffName = staff.FullName,
                EmployeeCode = staff.EmployeeCode ?? "",
                Month = month,
                Year = year,
                TotalActualDays = totalDays,
                Details = summaryDetails
            });
        }

        // ================================================================
        // GET api/attendance/group/{groupId} — Danh sách chấm công của đoàn
        // ================================================================
        [HttpGet("group/{groupId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.ViewAll")]
        public async Task<IActionResult> GetGroupAttendance(int groupId)
        {
            var records = await _context.ScheduleCalendars
                .Include(sc => sc.Staff)
                .Include(sc => sc.MedicalGroup)
                .Where(sc => sc.GroupId == groupId)
                .OrderBy(sc => sc.ExamDate).ThenBy(sc => sc.Staff!.FullName)
                .ToListAsync();

            return Ok(records.Select(sc => new
            {
                sc.CalendarId,
                StaffId = sc.StaffId,
                StaffName = sc.Staff?.FullName,
                EmployeeCode = sc.Staff?.EmployeeCode,
                sc.ExamDate,
                sc.CheckInTime,
                sc.CheckOutTime,
                sc.IsConfirmed,
                sc.Note
            }));
        }

        // ================================================================
        // HELPERS
        // ================================================================
        // Method TryDecodeQrToken cũ đã bị thay thế bởi QrService.ValidateSignedToken
    }
}
