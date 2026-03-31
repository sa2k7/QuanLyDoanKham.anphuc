using Microsoft.AspNetCore.Http;
using QuanLyDoanKham.API.DTOs;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Services.Auth
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Message, AuthResponseDto Data)> LoginAsync(LoginDto request);
        Task<(bool IsSuccess, string Message, AuthResponseDto Data)> RefreshTokenAsync(RefreshTokenDto request);
        Task<(bool IsSuccess, string Message)> ChangePasswordAsync(string username, ChangePasswordDto request);
        Task<(bool IsSuccess, string Message)> RequestResetAsync(ResetRequestDto request);
        Task<(bool IsSuccess, string Message)> ProcessResetAsync(ProcessResetDto dto);
    }
}
