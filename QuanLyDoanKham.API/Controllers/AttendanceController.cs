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

            // 1. Ưu tiên đoàn Open hôm nay
            var activeGroup = await _context.MedicalGroups
                .OrderBy(g => g.GroupId)
                .FirstOrDefaultAsync(g => g.ExamDate.Date == today && (g.Status == "Open" || g.Status == "InProgress"));

            // 2. Fallback: đoàn Open gần nhất — load về memory rồi sort
            if (activeGroup == null)
            {
                var openGroups = await _context.MedicalGroups
                    .Where(g => g.Status == "Open" || g.Status == "InProgress")
                    .ToListAsync();

                activeGroup = openGroups
                    .OrderBy(g => Math.Abs((g.ExamDate.Date - today).TotalDays))
                    .FirstOrDefault();
            }

            if (activeGroup == null)
            {
                var nextGroup = await _context.MedicalGroups
                    .Where(g => g.ExamDate.Date >= today)
                    .OrderBy(g => g.ExamDate)
                    .FirstOrDefaultAsync();

                if (nextGroup != null)
                    return NotFound(new { message = $"Không có đoàn đang mở. Đoàn tiếp theo: {nextGroup.GroupName} ({nextGroup.ExamDate:dd/MM/yyyy})." });

                return NotFound(new { message = "Không có đoàn khám nào đang diễn ra." });
            }

            var token = qrService.GenerateSignedToken(activeGroup.GroupId, 12);
            var baseUrl = !string.IsNullOrEmpty(origin) ? origin : (_configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173");
            var frontendUrl = $"{baseUrl}/checkin?token={Uri.EscapeDataString(token)}";
            var pngBase64 = qrService.GenerateQr(frontendUrl);

            return Ok(new
            {
                groupId = activeGroup.GroupId,
                groupName = activeGroup.GroupName,
                examDate = activeGroup.ExamDate,
                qrToken = token,
                qrUrl = frontendUrl,
                pngBase64 = pngBase64,
                expiresAt = DateTime.Now.AddHours(12),
                isToday = activeGroup.ExamDate.Date == today
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
        // POST api/attendance/checkin — Nhân viên quét QR → check-in/check-out
        // ================================================================
        [HttpPost("checkin")]
        [AllowAnonymous]  // Staff scan from phone without login session
        public async Task<IActionResult> CheckInByQr([FromBody] QrCheckInDto dto, [FromServices] Services.QrService qrService)
        {
            if (string.IsNullOrEmpty(dto.QrToken))
                return BadRequest(new { message = "QR token không hợp lệ." });

            // Validate token
            if (!qrService.ValidateSignedToken(dto.QrToken, out int groupId, out string error))
                return BadRequest(new { message = error });

            // Xác định nhân sự từ body hoặc JWT claim
            int staffId = dto.StaffId;
            if (staffId <= 0)
            {
                var staffIdClaim = User.FindFirst("StaffId")?.Value;
                if (int.TryParse(staffIdClaim, out var claimStaffId))
                    staffId = claimStaffId;

                // Try to get from JWT if authenticated
                if (staffId <= 0 && User.Identity?.IsAuthenticated == true)
                {
                    var username = User.Identity?.Name;
                    var staff = await _context.Staffs.FirstOrDefaultAsync(s =>
                        s.EmployeeCode != null && s.EmployeeCode.ToLower() == (username ?? "").ToLower());
                    if (staff != null) staffId = staff.StaffId;
                }

                if (staffId <= 0)
                    return BadRequest(new { message = "Vui lòng nhập Mã nhân viên (Staff ID) để chấm công." });
            }

            var group = await _context.MedicalGroups.FindAsync(groupId);
            if (group == null) return NotFound(new { message = "Không tìm thấy đoàn khám." });

            if (group.Status == "Locked" || group.Status == "Finished")
                return BadRequest(new { message = "Đoàn khám đã kết thúc." });

            var groupDetail = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(gd => gd.GroupId == groupId && gd.StaffId == staffId);
            if (groupDetail == null)
                return BadRequest(new { message = "Bạn không được phân công vào đoàn khám này." });

            var today = group.ExamDate.Date;
            var existing = await _context.ScheduleCalendars
                .FirstOrDefaultAsync(sc => sc.GroupId == groupId && sc.StaffId == staffId && sc.ExamDate.Date == today);

            var now = DateTime.Now;
            string action;

            if (existing == null || existing.CheckInTime == null)
            {
                // Check-in
                if (existing == null)
                {
                    existing = new ScheduleCalendar
                    {
                        GroupId = groupId,
                        StaffId = staffId,
                        ExamDate = today,
                        MedicalGroup = null!,
                        Staff = null!,
                        IsConfirmed = false,
                        Note = dto.Note ?? "QR Check-in"
                    };
                    _context.ScheduleCalendars.Add(existing);
                }
                existing.CheckInTime = now;
                groupDetail.CheckInTime = now;
                action = "CheckIn";
            }
            else if (existing.CheckOutTime == null)
            {
                // Check-out
                existing.CheckOutTime = now;
                groupDetail.CheckOutTime = now;
                var hours = (now - existing.CheckInTime!.Value).TotalHours;
                var shift = hours >= 4 ? 1.0 : 0.5;
                groupDetail.ShiftType = shift;
                existing.IsConfirmed = true;

                var staffEntity = await _context.Staffs.FindAsync(staffId);
                if (staffEntity != null)
                    groupDetail.CalculatedSalary = (decimal)(shift * (double)staffEntity.DailyRate);

                action = "CheckOut";
            }
            else
            {
                return BadRequest(new { message = "Bạn đã hoàn thành đủ công cho đoàn này hôm nay." });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = action == "CheckIn" ? $"Check-in thành công lúc {now:HH:mm}!" : $"Check-out thành công! Ca làm: {groupDetail.ShiftType} ngày.",
                action,
                groupId,
                groupName = group.GroupName,
                staffId,
                time = now,
                shiftType = groupDetail.ShiftType
            });
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
        [Authorize]
        public async Task<IActionResult> GetAttendanceSummary(int staffId, [FromQuery] int month, [FromQuery] int year)
        {
            var staffIdClaim = User.FindFirst("StaffId")?.Value;
            var canViewAll = User.HasClaim("permission", "ChamCong.ViewAll")
                || User.FindFirst("RoleId")?.Value == "1";
            var isOwnSummary = int.TryParse(staffIdClaim, out var currentStaffId) && currentStaffId == staffId;
            if (!canViewAll && !isOwnSummary) return Forbid();

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
        // GET api/attendance/summary-all?month=4&year=2026
        // Tổng hợp chấm công TẤT CẢ nhân sự trong tháng — dùng cho Admin/Manager
        // ================================================================
        [HttpGet("summary-all")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.ViewAll")]
        public async Task<IActionResult> GetAllStaffAttendanceSummary(
            [FromQuery] int month = 0,
            [FromQuery] int year = 0)
        {
            if (month == 0) month = DateTime.Now.Month;
            if (year == 0) year = DateTime.Now.Year;

            // Lấy tất cả GroupStaffDetails trong tháng có check-in (ScheduleCalendar)
            var schedules = await _context.ScheduleCalendars
                .Include(sc => sc.Staff)
                .Include(sc => sc.MedicalGroup)
                .Where(sc => sc.ExamDate.Month == month && sc.ExamDate.Year == year)
                .ToListAsync();

            var groupDetails = await _context.GroupStaffDetails
                .Where(gd => gd.MedicalGroup.ExamDate.Month == month
                          && gd.MedicalGroup.ExamDate.Year == year)
                .ToListAsync();

            // Group by nhân sự
            var staffSummaries = schedules
                .GroupBy(sc => sc.StaffId)
                .Select(g =>
                {
                    var firstRecord = g.First();
                    var staffName = firstRecord.Staff?.FullName ?? $"Staff #{g.Key}";
                    var employeeCode = firstRecord.Staff?.EmployeeCode ?? "";
                    var dailyRate = firstRecord.Staff?.DailyRate ?? 0;

                    var workdays = g.Select(sc =>
                    {
                        var gd = groupDetails.FirstOrDefault(x => x.GroupId == sc.GroupId && x.StaffId == sc.StaffId!.Value);
                        var shift = gd?.ShiftType ?? (sc.IsConfirmed ? 1.0 : 0.5);
                        return new
                        {
                            sc.GroupId,
                            GroupName = sc.MedicalGroup?.GroupName ?? $"Đoàn #{sc.GroupId}",
                            sc.ExamDate,
                            ShiftType = shift,
                            CheckInTime = sc.CheckInTime,
                            CheckOutTime = sc.CheckOutTime,
                            WorkStatus = sc.IsConfirmed ? "Đủ công" : (sc.CheckInTime != null ? "Chờ check-out" : "Vắng"),
                            EarnedAmount = (decimal)shift * dailyRate
                        };
                    }).ToList();

                    var totalShift = workdays.Sum(w => w.ShiftType);
                    var totalEarned = workdays.Sum(w => w.EarnedAmount);

                    return new
                    {
                        StaffId = g.Key,
                        StaffName = staffName,
                        EmployeeCode = employeeCode,
                        DailyRate = dailyRate,
                        TotalDays = totalShift,
                        TotalEarned = totalEarned,
                        Workdays = workdays.OrderBy(w => w.ExamDate).ToList()
                    };
                })
                .OrderBy(s => s.StaffName)
                .ToList();

            return Ok(new
            {
                Month = month,
                Year = year,
                TotalStaff = staffSummaries.Count,
                TotalPayroll = staffSummaries.Sum(s => s.TotalEarned),
                Staffs = staffSummaries
            });
        }

        // ================================================================
        // HELPERS
        // ================================================================
        // Method TryDecodeQrToken cũ đã bị thay thế bởi QrService.ValidateSignedToken

        // ================================================================
        // GET api/attendance/summary-my-groups?month=4&year=2026
        // GroupLeader: chỉ thấy attendance của các đoàn mình quản lý
        // Hiển thị TẤT CẢ nhân sự được phân công, kể cả chưa check-in
        // ================================================================
        [HttpGet("summary-my-groups")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.ViewAll")]
        public async Task<IActionResult> GetMyGroupsAttendanceSummary(
            [FromQuery] int month = 0,
            [FromQuery] int year = 0)
        {
            if (month == 0) month = DateTime.Now.Month;
            if (year == 0) year = DateTime.Now.Year;

            // Xác định Staff của user hiện tại qua EmployeeCode
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var currentStaff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.EmployeeCode != null
                    && s.EmployeeCode.ToLower() == username.ToLower());

            if (currentStaff == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ nhân sự." });

            // Lấy danh sách GroupId mà user này là GroupLeader (WorkPosition chứa "trưởng đoàn")
            var myGroupIds = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                .Where(gsd => gsd.StaffId == currentStaff.StaffId
                    && gsd.WorkPosition != null
                    && gsd.WorkPosition.ToLower().Contains("trưởng đoàn")
                    && gsd.MedicalGroup!.ExamDate.Month == month
                    && gsd.MedicalGroup!.ExamDate.Year == year)
                .Select(gsd => gsd.GroupId)
                .Distinct()
                .ToListAsync();

            if (!myGroupIds.Any())
                return Ok(new
                {
                    Month = month,
                    Year = year,
                    TotalStaff = 0,
                    TotalPayroll = 0m,
                    Staffs = new List<object>()
                });

            // Lấy TẤT CẢ GroupStaffDetails trong các đoàn của GroupLeader (kể cả chưa check-in)
            var allGroupDetails = await _context.GroupStaffDetails
                .Include(gd => gd.Staff)
                .Include(gd => gd.MedicalGroup)
                .Where(gd => myGroupIds.Contains(gd.GroupId))
                .ToListAsync();

            // Lấy ScheduleCalendars (check-in records) cho các đoàn đó
            var schedules = await _context.ScheduleCalendars
                .Include(sc => sc.MedicalGroup)
                .Where(sc => myGroupIds.Contains(sc.GroupId)
                    && sc.ExamDate.Month == month && sc.ExamDate.Year == year)
                .ToListAsync();

            // Group by nhân sự — dùng GroupStaffDetails làm nguồn chính (không bỏ sót ai)
            var staffSummaries = allGroupDetails
                .GroupBy(gd => gd.StaffId)
                .Select(g =>
                {
                    var firstDetail = g.First();
                    var staffName = firstDetail.Staff?.FullName ?? $"Staff #{g.Key}";
                    var employeeCode = firstDetail.Staff?.EmployeeCode ?? "";
                    var dailyRate = firstDetail.Staff?.DailyRate ?? 0;

                    var workdays = g.Select(gd =>
                    {
                        // Tìm schedule record tương ứng (nếu có)
                        var sc = schedules.FirstOrDefault(s => s.GroupId == gd.GroupId && s.StaffId == gd.StaffId);

                        // Xác định trạng thái
                        string workStatus;
                        if (sc == null || sc.CheckInTime == null)
                            workStatus = "Chưa chấm công";
                        else if (sc.CheckOutTime == null)
                            workStatus = "Đã check-in";
                        else
                            workStatus = "Đã hoàn thành";

                        var shift = gd.ShiftType;
                        // Nếu chưa check-out, không tính công
                        var earnedShift = (sc?.IsConfirmed == true) ? shift : 0.0;

                        return new
                        {
                            gd.GroupId,
                            GroupName = gd.MedicalGroup?.GroupName ?? $"Đoàn #{gd.GroupId}",
                            ExamDate = gd.MedicalGroup?.ExamDate ?? DateTime.MinValue,
                            ShiftType = shift,
                            CheckInTime = sc?.CheckInTime,
                            CheckOutTime = sc?.CheckOutTime,
                            WorkStatus = workStatus,
                            EarnedAmount = (decimal)earnedShift * dailyRate
                        };
                    }).ToList();

                    var totalDays = workdays.Where(w => w.WorkStatus == "Đã hoàn thành").Sum(w => w.ShiftType);
                    var totalEarned = workdays.Sum(w => w.EarnedAmount);

                    return new
                    {
                        StaffId = g.Key,
                        StaffName = staffName,
                        EmployeeCode = employeeCode,
                        DailyRate = dailyRate,
                        TotalDays = totalDays,
                        TotalEarned = totalEarned,
                        Workdays = workdays.OrderBy(w => w.ExamDate).ToList()
                    };
                })
                .OrderBy(s => s.StaffName)
                .ToList();

            return Ok(new
            {
                Month = month,
                Year = year,
                TotalStaff = staffSummaries.Count,
                TotalPayroll = staffSummaries.Sum(s => s.TotalEarned),
                Staffs = staffSummaries
            });
        }
        // ================================================================
        // ── NEW: SIMPLIFIED ATTENDANCE APIs (health-check-domain-refactor) ──
        // These 3 endpoints are ADDITIVE — no existing action is modified.
        // The existing POST /api/attendance/checkin (QR-based, AllowAnonymous)
        // is untouched. New endpoints use /staff/checkin and /staff/checkout
        // routes to avoid collision.
        // ================================================================

        // GET api/attendance/today
        // Returns all campaigns assigned to the current user for today.
        // Returns [] (never 404) when no assignments exist.
        [HttpGet("today")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.CheckInOut")]
        public async Task<IActionResult> GetTodayCampaigns()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var today = DateTime.Today;

            // Resolve staff from username (EmployeeCode or linked StaffId claim)
            var staffIdClaim = User.FindFirst("StaffId")?.Value;
            int? staffId = int.TryParse(staffIdClaim, out var sid) && sid > 0 ? sid : null;

            if (staffId == null)
            {
                var staff = await _context.Staffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.EmployeeCode != null
                        && s.EmployeeCode.ToLower() == username.ToLower());
                staffId = staff?.StaffId;
            }

            if (staffId == null)
                return Ok(new List<object>()); // No staff record → no assignments

            // Find all GroupStaffDetails for this staff on today's date
            var assignments = await _context.GroupStaffDetails
                .AsNoTracking()
                .Include(g => g.MedicalGroup)
                .Where(g => g.StaffId == staffId && g.ExamDate.Date == today)
                .ToListAsync();

            if (!assignments.Any())
                return Ok(new List<object>());

            var groupIds = assignments.Select(a => a.GroupId).ToList();

            // Fetch today's schedule records to determine check-in/out state
            var schedules = await _context.ScheduleCalendars
                .AsNoTracking()
                .Where(sc => groupIds.Contains(sc.GroupId)
                    && sc.StaffId == staffId
                    && sc.ExamDate.Date == today)
                .ToDictionaryAsync(sc => sc.GroupId);

            var result = assignments.Select(a =>
            {
                schedules.TryGetValue(a.GroupId, out var sc);
                return new AttendanceTodayDto
                {
                    CampaignId = a.GroupId,
                    CampaignName = a.MedicalGroup?.GroupName ?? $"Đoàn #{a.GroupId}",
                    ExamDate = a.MedicalGroup?.ExamDate ?? today,
                    CheckedIn = sc?.CheckInTime != null,
                    CheckedOut = sc?.CheckOutTime != null
                };
            }).ToList();

            return Ok(result);
        }

        // POST api/attendance/staff/checkin
        // Simplified check-in for the current authenticated user.
        // Returns 409 if already checked in for this campaign today.
        [HttpPost("staff/checkin")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.CheckInOut")]
        public async Task<IActionResult> StaffCheckIn([FromBody] AttendanceCheckInDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var today = DateTime.Today;
            var now = dto.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;

            // Resolve staffId
            var staffIdClaim = User.FindFirst("StaffId")?.Value;
            int? staffId = int.TryParse(staffIdClaim, out var sid) && sid > 0 ? sid : null;
            if (staffId == null)
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.EmployeeCode != null
                        && s.EmployeeCode.ToLower() == username.ToLower());
                staffId = staff?.StaffId;
            }
            if (staffId == null)
                return BadRequest(new { message = "Không tìm thấy hồ sơ nhân sự liên kết với tài khoản này." });

            // Verify assignment exists
            var assignment = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(g => g.GroupId == dto.CampaignId
                    && g.StaffId == staffId
                    && g.ExamDate.Date == today);
            if (assignment == null)
                return BadRequest(new { message = "Bạn không được phân công tham gia chiến dịch này hôm nay." });

            // Idempotency guard — cannot check-in twice
            var existing = await _context.ScheduleCalendars
                .FirstOrDefaultAsync(sc => sc.GroupId == dto.CampaignId
                    && sc.StaffId == staffId
                    && sc.ExamDate.Date == today);

            if (existing?.CheckInTime != null)
                return Conflict(new { message = "Bạn đã điểm danh vào ca này rồi" });

            if (existing == null)
            {
                _context.ScheduleCalendars.Add(new Models.ScheduleCalendar
                {
                    GroupId = dto.CampaignId,
                    StaffId = staffId,
                    ExamDate = today,
                    MedicalGroup = null!,
                    Staff = null!,
                    CheckInTime = now.ToLocalTime(),
                    IsConfirmed = false,
                    Note = "Điểm danh vào ca (simplified API)"
                });
            }
            else
            {
                existing.CheckInTime = now.ToLocalTime();
            }

            assignment.CheckInTime = now.ToLocalTime();
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Điểm danh vào ca thành công lúc {now.ToLocalTime():HH:mm}.", campaignId = dto.CampaignId });
        }

        // POST api/attendance/staff/checkout
        // Simplified check-out for the current authenticated user.
        // Returns 400 if not yet checked in; 409 if already checked out.
        [HttpPost("staff/checkout")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("ChamCong.CheckInOut")]
        public async Task<IActionResult> StaffCheckOut([FromBody] AttendanceCheckOutDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var today = DateTime.Today;
            var now = dto.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;

            // Resolve staffId
            var staffIdClaim = User.FindFirst("StaffId")?.Value;
            int? staffId = int.TryParse(staffIdClaim, out var sid) && sid > 0 ? sid : null;
            if (staffId == null)
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.EmployeeCode != null
                        && s.EmployeeCode.ToLower() == username.ToLower());
                staffId = staff?.StaffId;
            }
            if (staffId == null)
                return BadRequest(new { message = "Không tìm thấy hồ sơ nhân sự liên kết với tài khoản này." });

            var existing = await _context.ScheduleCalendars
                .FirstOrDefaultAsync(sc => sc.GroupId == dto.CampaignId
                    && sc.StaffId == staffId
                    && sc.ExamDate.Date == today);

            // Must have checked in first
            if (existing?.CheckInTime == null)
                return BadRequest(new { message = "Bạn chưa điểm danh vào ca, không thể điểm danh ra" });

            // Idempotency guard — cannot check-out twice
            if (existing.CheckOutTime != null)
                return Conflict(new { message = "Bạn đã điểm danh ra ca này rồi" });

            existing.CheckOutTime = now.ToLocalTime();
            existing.IsConfirmed = true;

            // Update GroupStaffDetail check-out and compute shift
            var assignment = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(g => g.GroupId == dto.CampaignId
                    && g.StaffId == staffId
                    && g.ExamDate.Date == today);
            if (assignment != null)
            {
                assignment.CheckOutTime = now.ToLocalTime();
                var hours = (existing.CheckOutTime.Value - existing.CheckInTime.Value).TotalHours;
                assignment.ShiftType = hours >= 4 ? 1.0 : 0.5;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Điểm danh ra ca thành công lúc {now.ToLocalTime():HH:mm}.", campaignId = dto.CampaignId });
        }
    }
}
