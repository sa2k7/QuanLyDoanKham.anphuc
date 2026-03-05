using System;

namespace QuanLyDoanKham.API.DTOs
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public int? CompanyId { get; set; }
        public string AvatarPath { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int? CompanyId { get; set; }
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; }
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Password { get; set; } // Nếu cung cấp thì đổi mật khẩu
        public int RoleId { get; set; }
        public int? CompanyId { get; set; }
        public string AvatarPath { get; set; }
    }

    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string AvatarPath { get; set; }
    }
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetRequestDto
    {
        public string Username { get; set; }
    }

    public class ProcessResetDto
    {
        public int Id { get; set; }
        public string NewPassword { get; set; }
    }
}
