using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PermissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("matrix")]
    [AuthorizePermission("HeThong.RoleManage")]
    public async Task<IActionResult> GetPermissionMatrix()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.PermissionId)
            .Select(p => new
            {
                p.PermissionId,
                p.PermissionKey,
                p.PermissionName,
                p.Module
            })
            .ToListAsync();

        // Group by Module
        var matrix = permissions.GroupBy(p => p.Module)
            .Select(g => new
            {
                Module = g.Key,
                Permissions = g.ToList()
            })
            .ToList();

        return Ok(matrix);
    }
}
