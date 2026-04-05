using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Auth
{
    public interface ITokenService
    {
        string CreateToken(AppUser user, List<string> roles, List<string> permissions);
        string GenerateSecureRefreshToken();
        string HashRefreshToken(string token);
    }
}
