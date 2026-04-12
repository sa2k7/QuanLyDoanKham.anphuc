using Microsoft.IdentityModel.Tokens;
using QuanLyDoanKham.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public class CheckInService : ICheckInService
    {
        private readonly IMedicalRecordStateMachine _stateMachine;
        private readonly IConfiguration _configuration;

        public CheckInService(IMedicalRecordStateMachine stateMachine, IConfiguration configuration)
        {
            _stateMachine = stateMachine;
            _configuration = configuration;
        }

        public async Task<ServiceResult<object>> ProcessCheckInTokenAsync(string qrToken, string actorUserId)
        {
            try
            {
                var principal = ValidateToken(qrToken);
                if (principal == null) return ServiceResult<object>.Failure("QR Token không hợp lệ hoặc đã hết hạn.");

                var typeClaim = principal.FindFirst("type")?.Value;
                if (typeClaim != "MEDICAL_RECORD_CHECKIN") return ServiceResult<object>.Failure("Loại Token không đúng.");

                var recordIdStr = principal.FindFirst("recordId")?.Value;
                if (!int.TryParse(recordIdStr, out var recordId)) return ServiceResult<object>.Failure("Dữ liệu hồ sơ trong Token không hợp lệ.");

                var result = await _stateMachine.CheckInAsync(recordId, actorUserId);
                if (!result.IsSuccess) return ServiceResult<object>.Failure(result.Message);

                return ServiceResult<object>.Success(new {
                    result.Data?.MedicalRecordId,
                    result.Data?.FullName,
                    result.Data?.QueueNo,
                    result.Data?.Status
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Failure("Lỗi xử lý Check-in: " + ex.Message);
            }
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            var configKey = _configuration.GetSection("AppSettings:Token").Value;
            if (string.IsNullOrEmpty(configKey)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configKey);

            try
            {
                // Simple validation for internal QR codes
                return tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham",
                    ValidateAudience = true,
                    ValidAudience = _configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QR AUTH ERROR] {ex.Message}");
                return null;
            }
        }
    }
}
