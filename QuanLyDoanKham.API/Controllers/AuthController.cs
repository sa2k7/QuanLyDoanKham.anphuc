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

        // GET: api/Auth/roles — Danh sách roles
        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.OrderBy(r => r.RoleId).ToListAsync();
            return Ok(roles);
        }

        // GET: api/Auth/permissions — Danh sách permissions theo module
        [HttpGet("permissions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPermissions()
        {
            var perms = await _context.Permissions
                .OrderBy(p => p.Module).ThenBy(p => p.PermissionId)
                .ToListAsync();
            return Ok(perms);
        }

        // GET: api/Auth/users
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Company)
                .Include(u => u.Department)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var list = users.Select(u => new UserProfileDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                RoleName = u.Role?.RoleName,
                RoleId = u.RoleId,
                Roles = u.UserRoles.Select(ur => ur.Role?.RoleName).Where(r => r != null).ToList(),
                CompanyId = u.CompanyId,
                CompanyName = u.Company?.CompanyName,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department?.DepartmentName,
                Email = u.Email,
                AvatarPath = u.AvatarPath,
                IsActive = u.IsActive
            }).ToList();

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
                    .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(u => u.Company)
                .Include(u => u.Department)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null) return NotFound();

            // Tổng hợp tất cả roles
            var allRoles = new HashSet<string>();
            if (user.Role != null) allRoles.Add(user.Role.RoleName);
            foreach (var ur in user.UserRoles) if (ur.Role != null) allRoles.Add(ur.Role.RoleName);

            // Tổng hợp tất cả permissions
            var allPerms = new HashSet<string>();
            if (user.Role?.RoleName == "Admin")
            {
                foreach (var p in await _context.Permissions.ToListAsync())
                    allPerms.Add(p.PermissionKey);
            }
            else
            {
                if (user.Role?.RolePermissions != null)
                    foreach (var rp in user.Role.RolePermissions)
                        if (rp.Permission != null) allPerms.Add(rp.Permission.PermissionKey);

                foreach (var ur in user.UserRoles ?? new List<UserRole>())
                    if (ur.Role?.RolePermissions != null)
                        foreach (var rp in ur.Role.RolePermissions)
                            if (rp.Permission != null) allPerms.Add(rp.Permission.PermissionKey);
            }

            return Ok(new UserProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                RoleName = user.Role?.RoleName,
                RoleId = user.RoleId,
                Roles = allRoles.ToList(),
                Permissions = allPerms.ToList(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.CompanyName,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.DepartmentName,
                Email = user.Email,
                AvatarPath = user.AvatarPath,
                IsActive = user.IsActive
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
            user.FullName = dto.FullName ?? user.FullName;
            user.RoleId = dto.RoleId > 0 ? dto.RoleId : user.RoleId;
            user.CompanyId = dto.CompanyId;
            user.DepartmentId = dto.DepartmentId;
            user.Email = dto.Email;
            user.AvatarPath = dto.AvatarPath ?? user.AvatarPath;
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Cập nhật multi-roles nếu có
            if (dto.RoleIds != null && dto.RoleIds.Count > 0)
            {
                var existing = _context.UserRoles.Where(ur => ur.UserId == user.UserId);
                _context.UserRoles.RemoveRange(existing);
                foreach (var rid in dto.RoleIds.Distinct())
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = rid,
                        AssignedAt = DateTime.Now,
                        AssignedBy = User.Identity?.Name
                    });
                }
            }

            await _context.SaveChangesAsync();

            if (oldRoleId != user.RoleId)
            {
                try
                {
                    var newRole = await _context.Roles.FindAsync(user.RoleId);
                    if (newRole != null)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = user.UserId,
                            Message = $"Vai trò của bạn đã được cập nhật thành: {newRole.RoleName}.",
                            Link = "/profile",
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Notification Error] UpdateUser: {ex.Message}");
                }
            }

            return Ok(new { message = "Cập nhật tài khoản thành công" });
        }

        // POST: api/Auth/assign-roles — Gán roles cho user
        [HttpPost("assign-roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("Không tìm thấy user.");

            // Xóa tất cả roles cũ
            var existingRoles = _context.UserRoles.Where(ur => ur.UserId == dto.UserId);
            _context.UserRoles.RemoveRange(existingRoles);

            // Gán roles mới
            foreach (var roleId in dto.RoleIds.Distinct())
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = dto.UserId,
                    RoleId = roleId,
                    AssignedAt = DateTime.Now,
                    AssignedBy = User.Identity?.Name
                });
            }

            // Đặt primary role là role đầu tiên trong danh sách
            if (dto.RoleIds.Count > 0)
                user.RoleId = dto.RoleIds[0];

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã gán {dto.RoleIds.Count} vai trò cho {user.Username}." });
        }

        // GET: api/Auth/role-permissions/{roleId} — Lấy permissions của role
        [HttpGet("role-permissions/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return NotFound();

            var perms = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => new { rp.Permission.PermissionId, rp.Permission.PermissionKey, rp.Permission.PermissionName, rp.Permission.Module })
                .ToListAsync();

            return Ok(perms);
        }

        // PUT: api/Auth/role-permissions/{roleId} — Cập nhật permissions cho role
        [HttpPut("role-permissions/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] AssignPermissionsDto dto)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return NotFound("Không tìm thấy role.");
            if (role.RoleName == "Admin") return BadRequest("Không thể sửa quyền của Admin.");

            // Xóa cũ
            var existing = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
            _context.RolePermissions.RemoveRange(existing);

            // Thêm mới
            foreach (var permId in dto.PermissionIds.Distinct())
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permId
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã cập nhật {dto.PermissionIds.Count} quyền cho role {role.RoleName}." });
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

            if (request.AdditionalRoleIds != null && request.AdditionalRoleIds.Count > 0)
            {
                foreach (var rid in request.AdditionalRoleIds.Distinct())
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = newUser.UserId,
                        RoleId = rid,
                        AssignedAt = DateTime.Now,
                        AssignedBy = User.Identity?.Name
                    });
                }
                await _context.SaveChangesAsync();
            }

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
                .Where(r => !r.IsProcessed)
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
