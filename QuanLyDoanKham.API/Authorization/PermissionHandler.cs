using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.API.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationDbContext _db;
    public PermissionHandler(ApplicationDbContext db) => _db = db;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // 1) Nháº­n quyá»n tá»« JWT claim (preferrable, khÃ´ng cáº§n DB)
        if (context.User.Claims.Any(c => c.Type == "permission" && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // 2) Kiá»ƒm tra Super Admin (Bypass táº¥t cáº£ náº¿u lÃ  Admin Root)
        var roleIdClaim = context.User.FindFirst("RoleId")?.Value;
        if (roleIdClaim == "1")
        {
            context.Succeed(requirement);
            return;
        }

        var userIdStr = context.User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return;

        // Fallback: Kiá»ƒm tra DB náº¿u claim RoleId bá»‹ thiáº¿u (Token cÅ©)
        var user = await _db.Users.FindAsync(userId);
        if (user != null && user.RoleId == 1)
        {
            context.Succeed(requirement);
            return;
        }

        // 3) Kiểm tra primary role
        var hasPerm = await _db.Users
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Role != null ? u.Role.RolePermissions.Select(rp => rp.Permission!.PermissionKey) : Enumerable.Empty<string>())
            .AnyAsync(pk => pk == requirement.Permission);

        // 3) Kiá»ƒm tra cÃ¡c role phá»¥ (UserRoles)
        if (!hasPerm)
        {
            hasPerm = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role != null ? ur.Role.RolePermissions.Select(rp => rp.Permission!.PermissionKey) : Enumerable.Empty<string>())
                .AnyAsync(pk => pk == requirement.Permission);
        }

        if (hasPerm)
        {
            context.Succeed(requirement);
        }
    }
}
