using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-100 ký tự.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự.")]
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8-100 ký tự.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2-200 ký tự.")]
        public string FullName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vai trò không hợp lệ.")]
        public int RoleId { get; set; }

        public int? CompanyId { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(200)]
        public string Email { get; set; }

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
        [Required]
        public string RefreshToken { get; set; }
    }

    public class UpdateUserDto
    {
        [StringLength(200, MinimumLength = 2)]
        public string FullName { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8-100 ký tự.")]
        public string Password { get; set; } // Nếu cung cấp thì đổi mật khẩu

        [Range(1, int.MaxValue, ErrorMessage = "Vai trò không hợp lệ.")]
        public int RoleId { get; set; }

        public int? CompanyId { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(200)]
        public string Email { get; set; }

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
        public string Email { get; set; }
        public string AvatarPath { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8-100 ký tự.")]
        public string NewPassword { get; set; }
    }

    public class ResetRequestDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string Username { get; set; }
    }

    public class ProcessResetDto
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8-100 ký tự.")]
        public string NewPassword { get; set; }
    }
}
