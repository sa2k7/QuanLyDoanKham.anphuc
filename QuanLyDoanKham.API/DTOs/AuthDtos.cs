using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(200, MinimumLength = 2)]
        public string FullName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vai trò không hợp lệ.")]
        public int RoleId { get; set; }

        /// <summary>Danh sách role IDs bổ sung (multi-role)</summary>
        public List<int> AdditionalRoleIds { get; set; } = new();

        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        public string AvatarPath { get; set; }
    }

    /// <summary>Response khi đăng nhập thành công - bao gồm roles[] và permissions[]</summary>
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }

        /// <summary>Primary role name (backward compat)</summary>
        public string Role { get; set; }

        /// <summary>Tất cả roles của user</summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>Tất cả permission keys của user (ví dụ: HopDong.Approve)</summary>
        public List<string> Permissions { get; set; } = new();

        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string AvatarPath { get; set; }
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

        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }

        /// <summary>Danh sách role IDs gán cho user (ghi đè toàn bộ)</summary>
        public List<int> RoleIds { get; set; } = new();

        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        public string AvatarPath { get; set; }

        public bool? IsActive { get; set; }
    }

    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }

        // Primary role (backward compat)
        public string RoleName { get; set; }
        public int RoleId { get; set; }

        /// <summary>Tất cả roles</summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>Tất cả permissions</summary>
        public List<string> Permissions { get; set; } = new();

        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Email { get; set; }
        public string AvatarPath { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }

    public class ResetRequestDto
    {
        [Required]
        public string Username { get; set; }
    }

    public class ProcessResetDto
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }

    // ---- User Role Management DTOs ----

    public class AssignRolesDto
    {
        [Required]
        public int UserId { get; set; }

        /// <summary>Danh sách RoleId cần gán (ghi đè toàn bộ)</summary>
        [Required]
        public List<int> RoleIds { get; set; } = new();
    }

    public class AssignPermissionsDto
    {
        /// <summary>Danh sách PermissionId cần gán cho role (ghi đè toàn bộ)</summary>
        [Required]
        public List<int> PermissionIds { get; set; } = new();
    }
}
