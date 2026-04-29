using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Services.Auth;

namespace QuanLyDoanKham.API.Controllers;

/// <summary>
/// Admin management endpoints for system administration tasks.
/// </summary>
[Route("api/admin")]
[ApiController]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly ITokenRefreshService _tokenRefreshService;
    private readonly IPermissionService _permissionService;
    private readonly ApplicationDbContext _context;

    public AdminController(
        ITokenRefreshService tokenRefreshService,
        IPermissionService permissionService,
        ApplicationDbContext context)
    {
        _tokenRefreshService = tokenRefreshService;
        _permissionService = permissionService;
        _context = context;
    }

    /// <summary>
    /// Invalidates all active refresh tokens, forcing all users to re-login.
    /// Use this after updating permissions to ensure users get fresh JWTs.
    /// </summary>
    [HttpPost("invalidate-all-tokens")]
    [AuthorizePermission("HeThong.UserManage")]
    public async Task<IActionResult> InvalidateAllTokens()
    {
        var count = await _tokenRefreshService.RefreshAllUserTokensAsync();
        return Ok(new
        {
            message = $"Đã vô hiệu hóa refresh token của {count} người dùng. Họ sẽ cần đăng nhập lại.",
            invalidatedCount = count
        });
    }

    /// <summary>
    /// Invalidates a specific user's refresh token, forcing them to re-login.
    /// </summary>
    [HttpPost("users/{userId}/invalidate-token")]
    [AuthorizePermission("HeThong.UserManage")]
    public async Task<IActionResult> InvalidateUserToken(int userId)
    {
        var success = await _tokenRefreshService.RefreshUserTokenAsync(userId);
        if (!success)
            return NotFound(new { message = $"Không tìm thấy người dùng với ID {userId}" });

        return Ok(new
        {
            message = $"Đã vô hiệu hóa refresh token của người dùng ID {userId}.",
            userId
        });
    }

    /// <summary>
    /// Returns a user's roles and permissions for debugging/auditing purposes.
    /// </summary>
    [HttpGet("users/{id}/permissions")]
    [AuthorizePermission("HeThong.UserManage")]
    public async Task<IActionResult> GetUserPermissions(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
            return NotFound(new { message = $"Không tìm thấy người dùng với ID {id}" });

        var permissions = await _permissionService.GetUserPermissionsAsync(id);

        var additionalRoles = user.UserRoles?
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role!.RoleName)
            .ToList() ?? new List<string>();

        return Ok(new
        {
            userId = user.UserId,
            username = user.Username,
            fullName = user.FullName,
            primaryRole = user.Role?.RoleName,
            additionalRoles,
            allRoles = new[] { user.Role?.RoleName }.Concat(additionalRoles).Where(r => r != null).Distinct(),
            permissions = permissions.OrderBy(p => p).ToList(),
            permissionCount = permissions.Count
        });
    }
}
