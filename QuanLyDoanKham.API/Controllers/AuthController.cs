using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.Auth;
using System.Security.Claims;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IAuthService authService)
        {
            _context = context;
            _configuration = configuration;
            _authService = authService;
        }

        // GET: api/Auth/users
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetUsers()
        {
            var list = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Company)
                .Select(u => new UserProfileDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    RoleName = u.Role.RoleName,
                    RoleId = u.RoleId,
                    CompanyId = u.CompanyId,
                    CompanyName = u.Company != null ? u.Company.CompanyName : null,
                    Email = u.Email,
                    AvatarPath = u.AvatarPath
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null) return NotFound();

            return Ok(new UserProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                RoleName = user.Role.RoleName,
                RoleId = user.RoleId,
                CompanyId = user.CompanyId,
                CompanyName = user.Company != null ? user.Company.CompanyName : null,
                Email = user.Email,
                AvatarPath = user.AvatarPath
            });
        }

        // PUT: api/Auth/users/{username}
        [HttpPut("users/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string username, UpdateUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            var oldRoleId = user.RoleId;
            user.FullName = dto.FullName;
            user.RoleId = dto.RoleId;
            user.CompanyId = dto.CompanyId;
            user.Email = dto.Email;
            user.AvatarPath = dto.AvatarPath;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            // TẠO THÔNG BÁO NẾU THAY ĐỔI VAI TRÒ
            if (oldRoleId != dto.RoleId)
            {
                try
                {
                    var newRole = await _context.Roles.FindAsync(dto.RoleId);
                    if (newRole != null)
                    {
                        var notification = new Notification
                        {
                            UserId = user.UserId,
                            Message = $"Vai trò hệ thống của bạn đã được cập nhật thành: {newRole.RoleName}.",
                            Link = "/profile",
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        };
                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi thông báo, không chặn luồng chính
                    Console.Error.WriteLine($"[Notification Error] UpdateUser: {ex.Message}");
                }
            }

            return Ok(new { message = "Cập nhật tài khoản thành công" });
        }

        // DELETE: api/Auth/users/{username}
        [HttpDelete("users/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            if (username == "admin") return BadRequest("Không thể xóa tài khoản Admin mặc định.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa tài khoản thành công" });
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            return Ok(result.Data);
        }

        // GET: api/Auth/verify-token
        [HttpGet("verify-token")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            // Nếu qua được [Authorize] nghĩa là token hợp lệ
            return Ok(new { valid = true, message = "Token hợp lệ" });
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserProfileDto>> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists.");

            var newUser = new AppUser
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                RoleId = request.RoleId,
                CompanyId = request.CompanyId,
                Email = request.Email,
                AvatarPath = request.AvatarPath
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Return DTO to avoid circular reference and match GET responses
            return Ok(new UserProfileDto
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                FullName = newUser.FullName,
                RoleId = newUser.RoleId,
                RoleName = (await _context.Roles.FindAsync(newUser.RoleId))?.RoleName,
                Email = newUser.Email,
                CompanyId = newUser.CompanyId
            });
        }

        // POST: api/Auth/upload-avatar
        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"uploads/avatars/{fileName}";
            return Ok(new { path = relativePath });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            return Ok(result.Data);
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var result = await _authService.ChangePasswordAsync(username, request);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // POST: api/Auth/request-reset
        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestReset([FromBody] ResetRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username)) 
                return BadRequest("Vui lòng cung cấp tên đăng nhập.");

            var result = await _authService.RequestResetAsync(request);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // GET: api/Auth/reset-requests
        [HttpGet("reset-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetResetRequests()
        {
            var list = await _context.PasswordResetRequests
                .OrderByDescending(r => r.RequestedDate)
                .Take(50)
                .ToListAsync();
            return Ok(list);
        }

        // POST: api/Auth/process-reset
        [HttpPost("process-reset")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessReset([FromBody] ProcessResetDto dto)
        {
            var result = await _authService.ProcessResetAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
