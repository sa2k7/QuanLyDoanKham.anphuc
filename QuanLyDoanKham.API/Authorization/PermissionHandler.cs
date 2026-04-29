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
        // 1) Check permission from JWT claim (fast path — no DB hit)
        if (context.User.Claims.Any(c => c.Type == "permission" && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // 2) Super Admin bypass (RoleId = 1 in JWT)
        var roleIdClaim = context.User.FindFirst("RoleId")?.Value;
        if (roleIdClaim == "1")
        {
            context.Succeed(requirement);
            return;
        }

        var userIdStr = context.User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return;

        // 3) Fallback DB check — verify Admin by DB in case JWT is stale
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return;
        if (user.RoleId == 1)
        {
            context.Succeed(requirement);
            return;
        }

        // 4) Check primary role permissions via JOIN (EF-translatable query)
        var hasPerm = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == user.RoleId
                      && rp.Permission != null
                      && rp.Permission.PermissionKey == requirement.Permission)
            .AnyAsync();

        // 5) Check additional roles (UserRoles many-to-many)
        if (!hasPerm)
        {
            var additionalRoleIds = await _db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (additionalRoleIds.Count > 0)
            {
                hasPerm = await _db.RolePermissions
                    .AsNoTracking()
                    .Where(rp => additionalRoleIds.Contains(rp.RoleId)
                              && rp.Permission != null
                              && rp.Permission.PermissionKey == requirement.Permission)
                    .AnyAsync();
            }
        }

        if (hasPerm)
        {
            context.Succeed(requirement);
        }
    }
}
