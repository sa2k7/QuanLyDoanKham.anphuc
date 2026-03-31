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
                var user = await _context.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == loginId || u.Email == loginId);

                // Unified message for security to prevent account enumeration
                string generalAuthError = "Thông tin đăng nhập không hợp lệ.";

                if (user == null) 
                    return (false, generalAuthError, null);

                bool isValid = false;
                try
                {
                    isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                }
                catch
                {
                    isValid = false;
                }

                if (!isValid) 
                    return (false, generalAuthError, null);

                if (user.Role == null && user.Username != "admin") 
                    return (false, "Tài khoản chưa được phân quyền trong hệ thống. Vui lòng liên hệ Admin.", null);
                
                if (user.Role == null && user.Username == "admin") 
                {
                    user.Role = await _context.Roles.FindAsync(1);
                }

                var token = CreateToken(user);
                
                // Refresh Token Creation with CSPRNG
                var originalRefreshToken = GenerateSecureRefreshToken();
                var refreshTokenHash = HashRefreshToken(originalRefreshToken);

                user.RefreshToken = refreshTokenHash;
                user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
                await _context.SaveChangesAsync();

                return (true, "Thành công", new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = originalRefreshToken, // Return raw token to client
                    Username = user.Username,
                    Role = user.Role.RoleName,
                    CompanyId = user.CompanyId
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Login Error in Service] {ex.Message}");
                throw; // Let standard exception handler deal with it
            }
        }

        public async Task<(bool IsSuccess, string Message, AuthResponseDto Data)> RefreshTokenAsync(RefreshTokenDto request)
        {
            var hashedInputToken = HashRefreshToken(request.RefreshToken);
            
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == hashedInputToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.Now)
                return (false, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.", null);

            if (user.Role == null) 
                return (false, "Tài khoản chưa được phân quyền.", null);

            var newToken = CreateToken(user);
            var newOriginalRefreshToken = GenerateSecureRefreshToken();
            var newRefreshTokenHash = HashRefreshToken(newOriginalRefreshToken);

            // Rotate Refresh Token
            user.RefreshToken = newRefreshTokenHash;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return (true, "Thành công", new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newOriginalRefreshToken, // Return raw to client
                Username = user.Username,
                Role = user.Role.RoleName,
                CompanyId = user.CompanyId
            });
        }

        public async Task<(bool IsSuccess, string Message)> ChangePasswordAsync(string username, ChangePasswordDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return (false, "Người dùng không tồn tại");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return (false, "Mật khẩu hiện tại không chính xác.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            
            // Invalidate current refresh token to enforce full re-login across devices
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

            var finalUsername = user.Username; 
            
            var existing = await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Username == finalUsername && !r.IsProcessed);
            
            if (existing != null) 
                return (true, "Yêu cầu của bạn đã được gửi đi và đang chờ Admin xử lý.");

            var resetRequest = new PasswordResetRequest
            {
                Username = finalUsername,
                RequestedDate = DateTime.Now
            };

            _context.PasswordResetRequests.Add(resetRequest);
            await _context.SaveChangesAsync();

            return (true, "Gửi yêu cầu thành công! Admin sẽ sớm phản hồi.");
        }

        public async Task<(bool IsSuccess, string Message)> ProcessResetAsync(ProcessResetDto dto)
        {
            var request = await _context.PasswordResetRequests.FindAsync(dto.Id);
            if (request == null) return (false, "Không tìm thấy yêu cầu.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null) return (false, "Không tìm thấy User tương ứng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            
            // Invalidate refresh token to log out current active sessions
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            request.IsProcessed = true;
            request.NewPassword = null; 

            await _context.SaveChangesAsync();
            return (true, $"Đã cấp lại mật khẩu cho {request.Username} thành công!");
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
            
            if (user.CompanyId.HasValue)
            {
                claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));
            }

            // Using proper configuration error handling per task spec P0
            var configKey = _configuration.GetSection("AppSettings:Token").Value;
            if (string.IsNullOrEmpty(configKey))
            {
                throw new Exception("CRITICAL: AppSettings:Token is missing from configuration!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham", // Or throw
                audience: _configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateSecureRefreshToken() // CSPRNG
        {
            var randomNumber = new byte[32]; // 256 bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        
        private string HashRefreshToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(token);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
