using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AuthorizePermission("HeThong.RoleManage")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .Select(r => new
            {
                r.RoleId,
                r.RoleName,
                r.Description
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("{id}/permissions")]
    [AuthorizePermission("HeThong.RoleManage")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return NotFound("Role not found.");

        var permissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        return Ok(permissionIds);
    }

    [HttpPost("{id}/permissions")]
    [AuthorizePermission("HeThong.RoleManage")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] int[] permissionIds)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return NotFound("Role not found.");

        // Remove old permissions
        var existingPermissions = await _context.RolePermissions.Where(rp => rp.RoleId == id).ToListAsync();
        _context.RolePermissions.RemoveRange(existingPermissions);

        // Add new permissions
        if (permissionIds != null && permissionIds.Length > 0)
        {
            var newRolePermissions = permissionIds.Select(pid => new RolePermission
            {
                RoleId = id,
                PermissionId = pid
            });
            _context.RolePermissions.AddRange(newRolePermissions);
        }

        // Add Audit Log
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int currentUserId))
        {
            var auditLog = new AuditLog
            {
                UserId = currentUserId,
                Action = "UPDATE_ROLE_PERMISSIONS",
                EntityType = "Role",
                EntityId = id,
                OldValue = string.Join(",", existingPermissions.Select(p => p.PermissionId)),
                NewValue = permissionIds != null ? string.Join(",", permissionIds) : "",
                Timestamp = DateTime.Now,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            };
            _context.AuditLogs.Add(auditLog);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật phân quyền thành công!" });
    }
}
