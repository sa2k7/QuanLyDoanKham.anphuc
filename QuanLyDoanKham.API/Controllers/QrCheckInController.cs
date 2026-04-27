using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>QR Code Check-in/Check-out cho nhân sự đoàn khám</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/checkin")]  // Alias for frontend compatibility
    [Authorize]
    public class QrCheckInController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public QrCheckInController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>Trưởng đoàn tạo QR code cho đoàn khám</summary>
        [HttpPost("generate/{groupId}")]
        [AuthorizePermission("ChamCong.QR")]
        public async Task<IActionResult> GenerateQrCode(int groupId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

            // Kiểm tra đoàn khám tồn tại
            var group = await _context.MedicalGroups.FindAsync(groupId);
            if (group == null) return NotFound("Không tìm thấy đoàn khám");

            // Kiểm tra người dùng có phải trưởng đoàn hoặc quản lý đoàn không
            var isManager = await _context.GroupStaffDetails
                .AnyAsync(gsd => gsd.GroupId == groupId
                    && gsd.StaffId == userId
                    && (gsd.WorkPosition == "Trưởng đoàn" || gsd.WorkPosition == "Tổ trưởng"));

            var isSystemManager = User.IsInRole("Admin") || User.IsInRole("MedicalGroupManager");

            if (!isManager && !isSystemManager)
                return Forbid("Bạn không có quyền tạo QR cho đoàn khám này. Chỉ trưởng đoàn hoặc quản lý mới có quyền.");

            // Tạo JWT token cho QR code (hết hạn sau 4 giờ)
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
                throw new InvalidOperationException("JWT Key is not configured or too short (minimum 32 characters required).");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("type", "qr_checkin"),
                new Claim("groupId", groupId.ToString()),
                new Claim("generatedBy", userId.ToString()),
                new Claim("examDate", group.ExamDate.ToString("yyyy-MM-dd")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "QuanLyDoanKham",
                audience: _configuration["Jwt:Audience"] ?? "QuanLyDoanKham",
                claims: claims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                qrToken = tokenString,
                groupId,
                groupName = group.GroupName,
                examDate = group.ExamDate,
                expiresAt = DateTime.Now.AddHours(8),
                message = "QR code đã được tạo. Hiển thị QR này cho nhân sự quét để chấm công."
            });
        }

        /// <summary>Nhân sự quét QR code để check-in</summary>
        [HttpPost("scan")]
        public async Task<IActionResult> ScanQrCode([FromBody] QrScanRequest request)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int staffId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

            if (string.IsNullOrEmpty(request.QrToken))
                return BadRequest(new { message = "QR token không hợp lệ" });

            try
            {
                // Validate JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"] ?? "your-256-bit-secret-key-here-min-32-chars!");

                tokenHandler.ValidateToken(request.QrToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "QuanLyDoanKham",
                    ValidAudience = _configuration["Jwt:Audience"] ?? "QuanLyDoanKham",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var groupId = int.Parse(jwtToken.Claims.First(c => c.Type == "groupId").Value);
                var examDate = DateTime.Parse(jwtToken.Claims.First(c => c.Type == "examDate").Value);

                // Kiểm tra nhân sự có trong đoàn khám không
                var staffDetail = await _context.GroupStaffDetails
                    .FirstOrDefaultAsync(gsd =>
                        gsd.GroupId == groupId &&
                        gsd.StaffId == staffId &&
                        gsd.ExamDate.Date == examDate.Date);

                if (staffDetail == null)
                {
                    return BadRequest(new
                    {
                        message = "Bạn không được phân công vào đoàn khám này. Vui lòng liên hệ trưởng đoàn."
                    });
                }

                var now = DateTime.Now;
                var result = new QrScanResult
                {
                    StaffId = staffId,
                    GroupId = groupId,
                    ExamDate = examDate,
                    ScanTime = now
                };

                // Xử lý check-in hoặc check-out
                if (request.Type == "in")
                {
                    if (staffDetail.CheckInTime.HasValue)
                    {
                        return BadRequest(new
                        {
                            message = "Bạn đã check-in rồi. Giờ vào: " + staffDetail.CheckInTime.Value.ToString("HH:mm:ss")
                        });
                    }

                    staffDetail.CheckInTime = now;
                    result.Type = "checkin";
                    result.Message = $"Check-in thành công! Giờ vào: {now:HH:mm:ss}";
                }
                else if (request.Type == "out")
                {
                    if (!staffDetail.CheckInTime.HasValue)
                    {
                        return BadRequest(new
                        {
                            message = "Bạn chưa check-in. Vui lòng check-in trước khi check-out."
                        });
                    }

                    if (staffDetail.CheckOutTime.HasValue)
                    {
                        return BadRequest(new
                        {
                            message = "Bạn đã check-out rồi. Giờ ra: " + staffDetail.CheckOutTime.Value.ToString("HH:mm:ss")
                        });
                    }

                    staffDetail.CheckOutTime = now;
                    result.Type = "checkout";

                    // Tính số giờ làm việc
                    var hoursWorked = (now - staffDetail.CheckInTime.Value).TotalHours;
                    result.HoursWorked = hoursWorked;

                    // Tính ca làm (0.5 = nửa ngày, 1.0 = cả ngày)
                    if (hoursWorked >= 4)
                    {
                        staffDetail.ShiftType = 1.0; // Cả ngày
                        result.ShiftType = 1.0;
                        result.Message = $"Check-out thành công! Tổng thời gian: {hoursWorked:F1} giờ. Tính công: 1 ngày.";
                    }
                    else
                    {
                        staffDetail.ShiftType = 0.5; // Nửa ngày
                        result.ShiftType = 0.5;
                        result.Message = $"Check-out thành công! Tổng thời gian: {hoursWorked:F1} giờ. Tính công: 0.5 ngày.";
                    }

                    // Tính lương tạm (sẽ được tính chi tiết hơn ở service khác)
                    var staff = await _context.Staffs.FindAsync(staffId);
                    if (staff != null)
                    {
                        staffDetail.CalculatedSalary = (decimal)(staffDetail.ShiftType * (double)staff.DailyRate);
                        result.CalculatedSalary = staffDetail.CalculatedSalary;
                    }
                }
                else
                {
                    return BadRequest(new { message = "Loại scan không hợp lệ. Chỉ chấp nhận 'in' hoặc 'out'" });
                }

                await _context.SaveChangesAsync();

                // Cập nhật hoặc tạo ScheduleCalendar
                var schedule = await _context.ScheduleCalendars
                    .FirstOrDefaultAsync(sc =>
                        sc.GroupId == groupId &&
                        sc.StaffId == staffId);

                if (schedule == null)
                {
                    schedule = new ScheduleCalendar
                    {
                        GroupId = groupId,
                        StaffId = staffId,
                        ExamDate = examDate
                    };
                    _context.ScheduleCalendars.Add(schedule);
                }

                if (request.Type == "in")
                    schedule.CheckInTime = now;
                else
                    schedule.CheckOutTime = now;

                schedule.IsConfirmed = true;
                await _context.SaveChangesAsync();

                return Ok(result);
            }
            catch (SecurityTokenExpiredException)
            {
                return BadRequest(new { message = "QR code đã hết hạn. Vui lòng yêu cầu trưởng đoàn tạo QR mới." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "QR code không hợp lệ: " + ex.Message });
            }
        }

        /// <summary>Lấy danh sách chấm công của đoàn khám</summary>
        [HttpGet("attendance/{groupId}")]
        [AuthorizePermission("ChamCong.ViewAll")]
        public async Task<IActionResult> GetAttendance(int groupId)
        {
            var attendance = await _context.GroupStaffDetails
                .Where(gsd => gsd.GroupId == groupId)
                .Include(gsd => gsd.Staff)
                .Select(gsd => new
                {
                    gsd.StaffId,
                    StaffName = gsd.Staff.FullName,
                    gsd.WorkPosition,
                    gsd.CheckInTime,
                    gsd.CheckOutTime,
                    gsd.ShiftType,
                    gsd.CalculatedSalary,
                    Status = gsd.CheckInTime == null ? "Chưa check-in" :
                             gsd.CheckOutTime == null ? "Đang làm việc" : "Hoàn thành"
                })
                .ToListAsync();

            var summary = new
            {
                TotalStaff = attendance.Count,
                CheckedIn = attendance.Count(a => a.CheckInTime != null),
                CheckedOut = attendance.Count(a => a.CheckOutTime != null),
                TotalWorkingDays = attendance.Sum(a => a.ShiftType),
                TotalSalary = attendance.Sum(a => a.CalculatedSalary)
            };

            return Ok(new { summary, attendance });
        }

        /// <summary>Check-in thủ công (cho admin hoặc trưởng đoàn)</summary>
        [HttpPost("manual-checkin")]
        [AuthorizePermission("ChamCong.CheckInOut")]
        public async Task<IActionResult> ManualCheckIn([FromBody] ManualCheckInRequest request)
        {
            var staffDetail = await _context.GroupStaffDetails
                .FirstOrDefaultAsync(gsd =>
                    gsd.GroupId == request.GroupId &&
                    gsd.StaffId == request.StaffId);

            if (staffDetail == null)
                return NotFound("Không tìm thấy phân công nhân sự trong đoàn khám");

            if (request.Type == "in")
            {
                if (request.Time.HasValue)
                    staffDetail.CheckInTime = request.Time.Value;
                else
                    staffDetail.CheckInTime = DateTime.Now;
            }
            else if (request.Type == "out")
            {
                if (request.Time.HasValue)
                    staffDetail.CheckOutTime = request.Time.Value;
                else
                    staffDetail.CheckOutTime = DateTime.Now;

                // Tính công nếu check-out
                if (staffDetail.CheckInTime.HasValue)
                {
                    var hours = (staffDetail.CheckOutTime.Value - staffDetail.CheckInTime.Value).TotalHours;
                    staffDetail.ShiftType = hours >= 4 ? 1.0 : 0.5;

                    var staff = await _context.Staffs.FindAsync(request.StaffId);
                    if (staff != null)
                        staffDetail.CalculatedSalary = (decimal)(staffDetail.ShiftType * (double)staff.DailyRate);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã cập nhật {request.Type} cho nhân sự",
                staffId = request.StaffId,
                groupId = request.GroupId,
                checkInTime = staffDetail.CheckInTime,
                checkOutTime = staffDetail.CheckOutTime,
                shiftType = staffDetail.ShiftType
            });
        }
    }

    public class QrScanRequest
    {
        public string QrToken { get; set; } = null!;
        public string Type { get; set; } = null!; // "in" hoặc "out"
    }

    public class QrScanResult
    {
        public int StaffId { get; set; }
        public int GroupId { get; set; }
        public DateTime ExamDate { get; set; }
        public string Type { get; set; } = null!;
        public DateTime ScanTime { get; set; }
        public double? HoursWorked { get; set; }
        public double ShiftType { get; set; }
        public decimal CalculatedSalary { get; set; }
        public string Message { get; set; } = null!;
    }

    public class ManualCheckInRequest
    {
        public int GroupId { get; set; }
        public int StaffId { get; set; }
        public string Type { get; set; } = null!; // "in" hoặc "out"
        public DateTime? Time { get; set; }
    }
}
