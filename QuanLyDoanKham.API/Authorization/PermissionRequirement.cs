using Microsoft.AspNetCore.Authorization;

namespace QuanLyDoanKham.API.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission) => Permission = permission;
    public string Permission { get; }
}
