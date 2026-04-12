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
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task<(bool IsSuccess, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto request)
        {
            try
            {
                var loginId = request.Username?.Trim();
                var user = await _context.Users
                    .Include(u => u.Role).ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
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

                // Đảm bảo primary role luôn có (Đã được seed trong DB, không hard-code ID)
                if (user.Role == null)
                    return (false, "Tài khoản chưa được phân quyền. Vui lòng liên hệ Admin.", null);

                // Tổng hợp tất cả roles + permissions
                var allRoles = GetAllRoles(user);
                var allPermissions = GetAllPermissions(user);

                var token = _tokenService.CreateToken(user, allRoles, allPermissions);
                var originalRefreshToken = _tokenService.GenerateSecureRefreshToken();
                var refreshTokenHash = _tokenService.HashRefreshToken(originalRefreshToken);

                user.RefreshToken = refreshTokenHash;
                user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
                await _context.SaveChangesAsync();

                return (true, "Thành công", new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = originalRefreshToken,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role!.RoleName,
                    Roles = allRoles,
                    Permissions = allPermissions,
                    CompanyId = user.CompanyId,
                    AvatarPath = user.AvatarPath
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Login Error] {ex.Message}");
                throw;
            }
        }

        public async Task<(bool IsSuccess, string Message, AuthResponseDto? Data)> RefreshTokenAsync(RefreshTokenDto request)
        {
            var hashedInputToken = _tokenService.HashRefreshToken(request.RefreshToken);

            var user = await _context.Users
                .Include(u => u.Role).ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.RefreshToken == hashedInputToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.Now)
                return (false, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.", null);

            if (user.Role == null)
                return (false, "Tài khoản chưa được phân quyền.", null);

            var allRoles = GetAllRoles(user);
            var allPermissions = GetAllPermissions(user);

            var newToken = _tokenService.CreateToken(user, allRoles, allPermissions);
            var newOriginalRefreshToken = _tokenService.GenerateSecureRefreshToken();
            var newRefreshTokenHash = _tokenService.HashRefreshToken(newOriginalRefreshToken);

            user.RefreshToken = newRefreshTokenHash;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return (true, "Thành công", new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newOriginalRefreshToken,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role!.RoleName,
                Roles = allRoles,
                Permissions = allPermissions,
                CompanyId = user.CompanyId,
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
            req.NewPassword = string.Empty;

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


    }
}
