using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuanLyDoanKham.API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool IsSuccess, string Message, AuthResponseDto Data)> LoginAsync(LoginDto request)
        {
            try
            {
                var loginId = request.Username?.Trim();
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Username == loginId || u.Email == loginId);

                const string generalAuthError = "Thông tin đăng nhập không hợp lệ.";

                if (user == null)
                    return (false, generalAuthError, null);

                if (!user.IsActive)
                    return (false, "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ Admin.", null);

                bool isValid = false;
                try { isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash); }
                catch { isValid = false; }

                if (!isValid)
                    return (false, generalAuthError, null);

                // Đảm bảo primary role luôn có
                if (user.Role == null && user.Username == "admin")
                    user.Role = await _context.Roles.FindAsync(1);

                if (user.Role == null)
                    return (false, "Tài khoản chưa được phân quyền. Vui lòng liên hệ Admin.", null);

                // Tổng hợp tất cả roles + permissions
                var allRoles = GetAllRoles(user);
                var allPermissions = GetAllPermissions(user);

                var token = CreateToken(user, allRoles, allPermissions);
                var originalRefreshToken = GenerateSecureRefreshToken();
                var refreshTokenHash = HashRefreshToken(originalRefreshToken);

                user.RefreshToken = refreshTokenHash;
                user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
                await _context.SaveChangesAsync();

                return (true, "Thành công", new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = originalRefreshToken,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role.RoleName,
                    Roles = allRoles,
                    Permissions = allPermissions,
                    CompanyId = user.CompanyId,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.DepartmentName,
                    AvatarPath = user.AvatarPath
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Login Error] {ex.Message}");
                throw;
            }
        }

        public async Task<(bool IsSuccess, string Message, AuthResponseDto Data)> RefreshTokenAsync(RefreshTokenDto request)
        {
            var hashedInputToken = HashRefreshToken(request.RefreshToken);

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.RefreshToken == hashedInputToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.Now)
                return (false, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.", null);

            if (user.Role == null)
                return (false, "Tài khoản chưa được phân quyền.", null);

            var allRoles = GetAllRoles(user);
            var allPermissions = GetAllPermissions(user);

            var newToken = CreateToken(user, allRoles, allPermissions);
            var newOriginalRefreshToken = GenerateSecureRefreshToken();
            var newRefreshTokenHash = HashRefreshToken(newOriginalRefreshToken);

            user.RefreshToken = newRefreshTokenHash;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return (true, "Thành công", new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newOriginalRefreshToken,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role.RoleName,
                Roles = allRoles,
                Permissions = allPermissions,
                CompanyId = user.CompanyId,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.DepartmentName,
                AvatarPath = user.AvatarPath
            });
        }

        public async Task<(bool IsSuccess, string Message)> ChangePasswordAsync(string username, ChangePasswordDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return (false, "Người dùng không tồn tại");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return (false, "Mật khẩu hiện tại không chính xác.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _context.SaveChangesAsync();
            return (true, "Đổi mật khẩu thành công!");
        }

        public async Task<(bool IsSuccess, string Message)> RequestResetAsync(ResetRequestDto request)
        {
            var username = request.Username?.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null)
                return (false, "Tài khoản không tồn tại trong hệ thống.");

            var existing = await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Username == user.Username && !r.IsProcessed);

            if (existing != null)
                return (true, "Yêu cầu của bạn đang chờ Admin xử lý.");

            _context.PasswordResetRequests.Add(new PasswordResetRequest
            {
                Username = user.Username,
                RequestedDate = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return (true, "Gửi yêu cầu thành công! Admin sẽ sớm phản hồi.");
        }

        public async Task<(bool IsSuccess, string Message)> ProcessResetAsync(ProcessResetDto dto)
        {
            var req = await _context.PasswordResetRequests.FindAsync(dto.Id);
            if (req == null) return (false, "Không tìm thấy yêu cầu.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null) return (false, "Không tìm thấy User tương ứng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            req.IsProcessed = true;
            req.NewPassword = null;

            await _context.SaveChangesAsync();
            return (true, $"Đã cấp lại mật khẩu cho {req.Username} thành công!");
        }

        // ================================================================
        // HELPERS
        // ================================================================

        /// <summary>Tổng hợp tất cả RoleName của user (primary + multi-role)</summary>
        private List<string> GetAllRoles(AppUser user)
        {
            var roles = new HashSet<string>();

            if (user.Role != null)
                roles.Add(user.Role.RoleName);

            foreach (var ur in user.UserRoles ?? new List<UserRole>())
                if (ur.Role != null)
                    roles.Add(ur.Role.RoleName);

            return roles.ToList();
        }

        /// <summary>Tổng hợp tất cả PermissionKey từ mọi role của user</summary>
        private List<string> GetAllPermissions(AppUser user)
        {
            var permissions = new HashSet<string>();

            // Admin gets all permissions
            if (user.Role?.RoleName == "Admin")
            {
                var allPerms = _context.Permissions.Select(p => p.PermissionKey).ToList();
                foreach (var p in allPerms) permissions.Add(p);
                return permissions.ToList();
            }

            // Từ primary role
            if (user.Role?.RolePermissions != null)
                foreach (var rp in user.Role.RolePermissions)
                    if (rp.Permission != null)
                        permissions.Add(rp.Permission.PermissionKey);

            // Từ các role phụ (UserRoles)
            foreach (var ur in user.UserRoles ?? new List<UserRole>())
                if (ur.Role?.RolePermissions != null)
                    foreach (var rp in ur.Role.RolePermissions)
                        if (rp.Permission != null)
                            permissions.Add(rp.Permission.PermissionKey);

            return permissions.ToList();
        }

        /// <summary>Tạo JWT token với đầy đủ roles + permissions claims</summary>
        private string CreateToken(AppUser user, List<string> roles, List<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim("FullName", user.FullName ?? user.Username ?? "User"),
                new Claim("UserId", user.UserId.ToString())
            };

            // Thêm tất cả roles vào claims
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Thêm tất cả permissions vào claims
            foreach (var perm in permissions)
                claims.Add(new Claim("permission", perm));

            if (user.CompanyId.HasValue)
                claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));

            if (user.DepartmentId.HasValue)
                claims.Add(new Claim("DepartmentId", user.DepartmentId.Value.ToString()));

            var configKey = _configuration.GetSection("AppSettings:Token").Value
                ?? throw new InvalidOperationException("CRITICAL: AppSettings:Token is missing!");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham",
                audience: _configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateSecureRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashRefreshToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }
    }
}
