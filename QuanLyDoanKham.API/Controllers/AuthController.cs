using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

        // POST: api/Auth/reset-admin-fix-emergency
        // CHỈ DÙNG TRONG TRƯỜNG HỢP KHẨN CẤP. XÓA SAU KHI DÙNG.
        [HttpPost("reset-admin-fix-emergency")]
        public async Task<IActionResult> ResetAdminFixEmergency()
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin == null) return NotFound("Không tìm thấy Admin.");

            // Ép hash về "admin123" chuẩn
            admin.PasswordHash = "$2a$11$azxY7U9nnOEV4xF4Cto.gOLsbvUKtALtNoqMjMFRhVfP/45V1BrJ."; 
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã khôi phục mật khẩu Admin thành 'admin123' thành công! Hãy đăng nhập lại ngay." });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
        {
            try
            {
                // 1. Check User (Username or Email) - Trim input for robustness
                var loginId = request.Username?.Trim();
                var user = await _context.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == loginId || u.Email == loginId);

                if (user == null) return Unauthorized("Tài khoản không tồn tại.");

                // 2. Check Password
                bool isValid = false;
                try
                {
                    isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                }
                catch
                {
                    isValid = false;
                }

                if (!isValid) return Unauthorized("Mật khẩu không chính xác.");

                if (user.Role == null && user.Username != "admin") return BadRequest("Tài khoản chưa được phân quyền trong hệ thống. Vui lòng liên hệ Admin.");
                if (user.Role == null && user.Username == "admin") 
                {
                    // Tự phục hồi role Admin nếu Db bị lỗi liên kết
                    user.Role = await _context.Roles.FindAsync(1);
                }

                // 4. Generate JWT Token and Refresh Token
                var token = CreateToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
                await _context.SaveChangesAsync();

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Username = user.Username,
                    Role = user.Role.RoleName,
                    CompanyId = user.CompanyId
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Login Error] {ex.Message}");
                return StatusCode(500, new { message = "Lỗi hệ thống. Vui lòng thử lại sau." });
            }
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
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.Now)
                return Unauthorized("Invalid or expired refresh token.");

            // BUG FIX: Check Role BEFORE SaveChanges to avoid race condition (DB sync with invalid role)
            if (user.Role == null) return Unauthorized("Tài khoản chưa được phân quyền.");

            var newToken = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Username = user.Username,
                Role = user.Role.RoleName,
                CompanyId = user.CompanyId
            });
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Mật khẩu hiện tại không chính xác.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        // POST: api/Auth/request-reset
        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestReset([FromBody] ResetRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username)) 
                return BadRequest("Vui lòng cung cấp tên đăng nhập.");

            var username = request.Username;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == username);
            if (user == null) return NotFound("Tài khoản không tồn tại trong hệ thống.");

            var finalUsername = user.Username; // Luôn dùng Username chính thức để lưu request
            
            // Check if there's already a pending request
            var existing = await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Username == finalUsername && !r.IsProcessed);
            
            if (existing != null) 
                return Ok(new { message = "Yêu cầu của bạn đã được gửi đi và đang chờ Admin xử lý." });

            var resetRequest = new PasswordResetRequest
            {
                Username = finalUsername,
                RequestedDate = DateTime.Now
            };

            _context.PasswordResetRequests.Add(resetRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi yêu cầu thành công! Admin sẽ sớm phản hồi." });
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
            var request = await _context.PasswordResetRequests.FindAsync(dto.Id);
            if (request == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null) return NotFound("Không tìm thấy User tương ứng.");

            // Update Password - chỉ lưu hash, KHÔNG lưu plaintext
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            
            // Mark as processed - KHÔNG ghi lại NewPassword thô
            request.IsProcessed = true;
            request.NewPassword = null; // Xóa sạch mật khẩu thô khỏi DB

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã cấp lại mật khẩu cho {request.Username} thành công!" });
        }

        private string CreateToken(AppUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Guest"),
                new Claim("FullName", user.FullName ?? user.Username ?? "User"),
                new Claim("UserId", user.UserId.ToString())
            };
            
            // Nếu là customer thì thêm CompanyId vào claim để filter dữ liệu
            if (user.CompanyId.HasValue)
            {
                claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));
            }

            // Key từ appsettings (hoặc hardcode cho demo)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value ?? "MySuperSecretKeyForGraduationProject2026!"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham",
                audience: _configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + 
                   Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
