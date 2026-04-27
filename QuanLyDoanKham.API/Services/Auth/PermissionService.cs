using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Auth;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var permissions = new HashSet<string>();

        var user = await _context.Users
            .Include(u => u.Role).ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return new List<string>();

        // From primary role
        if (user.Role?.RolePermissions != null)
            foreach (var rp in user.Role.RolePermissions)
                if (rp.Permission != null)
                    permissions.Add(rp.Permission.PermissionKey);

        // From additional roles (UserRoles)
        foreach (var ur in user.UserRoles ?? new List<UserRole>())
            if (ur.Role?.RolePermissions != null)
                foreach (var rp in ur.Role.RolePermissions)
                    if (rp.Permission != null)
                        permissions.Add(rp.Permission.PermissionKey);

        return permissions.ToList();
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
    {
        // Admin (RoleId == 1) bypasses all permission checks
        var user = await _context.Users.FindAsync(userId);
        if (user?.RoleId == 1) return true;

        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionKey);
    }
}
