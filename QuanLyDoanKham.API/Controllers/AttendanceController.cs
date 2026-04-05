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

            // Token = Base64(groupId + ":" + timestamp_unix) — hết hạn sau 12 giờ
            var expiry = DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds();
            var raw = $"{groupId}:{expiry}";
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

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
                expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiry).LocalDateTime,
                qrUrl = frontendUrl,
                pngBase64 = pngBase64
            });
        }

        // ================================================================
        // POST api/attendance/checkin — Nhân viên quét QR → check-in
        // ================================================================
        [HttpPost("checkin")]
        [AllowAnonymous] // Nhân viên chưa đăng nhập vẫn có thể checkin qua QR
        public async Task<IActionResult> CheckIn([FromBody] CheckInOutDto dto)
        {
            // Validate QR token
            if (string.IsNullOrEmpty(dto.QrToken))
                return BadRequest("Thiếu QR token.");

            if (!TryDecodeQrToken(dto.QrToken, out var tokenGroupId, out var expiry))
                return BadRequest("QR token không hợp lệ.");

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
                return BadRequest("QR code đã hết hạn. Vui lòng yêu cầu trưởng đoàn tạo mã mới.");

            if (tokenGroupId != dto.GroupId)
                return BadRequest("QR token không khớp với đoàn khám.");

            var group = await _context.MedicalGroups.FindAsync(dto.GroupId);
            if (group == null) return NotFound("Không tìm thấy đoàn khám.");

            // Xác định Staff từ token đăng nhập hoặc StaffId truyền vào
            int staffId = dto.StaffId ?? 0;
            if (staffId == 0)
            {
                var username = User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode == username.ToUpper());
                    staffId = staff?.StaffId ?? 0;
                }
            }
            if (staffId == 0) return BadRequest("Không xác định được nhân viên. Vui lòng đăng nhập.");

            // Kiểm tra nhân viên có trong đoàn không
            var groupDetail = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(gd => gd.GroupId == dto.GroupId && gd.StaffId == staffId);
            if (groupDetail == null)
                return Forbid();

            // Tìm bản ghi chấm công ngày hôm nay
            var today = DateTime.Today;
            var existing = await _context.ScheduleCalendars
                .FirstOrDefaultAsync(sc => sc.GroupId == dto.GroupId && sc.StaffId == staffId
                    && sc.ExamDate.Date == today);

            if (existing == null)
            {
                // Check-in lần đầu
                _context.ScheduleCalendars.Add(new ScheduleCalendar
                {
                    GroupId = dto.GroupId,
                    StaffId = staffId,
                    ExamDate = today,
                    CheckInTime = DateTime.Now,
                    IsConfirmed = false,
                    Note = dto.Note
                });
                await _context.SaveChangesAsync();
                return Ok(new { message = "Check-in thành công!", action = "CheckIn", time = DateTime.Now });
            }
            else if (existing.CheckOutTime == null)
            {
                // Check-out
                existing.CheckOutTime = DateTime.Now;

                // Tính ShiftType: 0.5 nếu < 4h, 1.0 nếu >= 4h
                var hours = (existing.CheckOutTime.Value - existing.CheckInTime.Value!).TotalHours;
                var resolvedShift = hours >= 4 ? 1.0 : 0.5;
                groupDetail.ShiftType = resolvedShift;

                existing.IsConfirmed = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Check-out thành công! Công quy đổi: {resolvedShift} công", action = "CheckOut", time = DateTime.Now, shiftType = resolvedShift });
            }
            else
            {
                return BadRequest("Nhân viên đã check-in và check-out trong ngày này rồi.");
            }
        }

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
                var hours = (existing.CheckOutTime.Value - existing.CheckInTime.Value!).TotalHours;
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
                EmployeeCode = staff.EmployeeCode,
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
                .OrderBy(sc => sc.ExamDate).ThenBy(sc => sc.Staff.FullName)
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
        private bool TryDecodeQrToken(string token, out int groupId, out long expiry)
        {
            groupId = 0; expiry = 0;
            try
            {
                var raw = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = raw.Split(':');
                if (parts.Length != 2) return false;
                if (!int.TryParse(parts[0], out groupId)) return false;
                if (!long.TryParse(parts[1], out expiry)) return false;
                return true;
            }
            catch { return false; }
        }
    }
}
