using Microsoft.AspNetCore.Authorization;

namespace QuanLyDoanKham.API.Middleware
{
    /// <summary>
    /// Requirement yêu cầu một permission key cụ thể.
    /// Dùng: [Authorize(Policy = "HopDong.Approve")]
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionKey { get; }

        public PermissionRequirement(string permissionKey)
        {
            PermissionKey = permissionKey;
        }
    }

    /// <summary>
    /// Handler kiểm tra claim "permission" trong JWT token.
    /// Lưu ý: KHÔNG bypass Admin (tuân thủ nguyên tắc PoLP).
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Kiểm tra claim "permission"
            var hasPerm = context.User.Claims
                .Any(c => c.Type == "permission" && c.Value == requirement.PermissionKey);

            if (hasPerm)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
