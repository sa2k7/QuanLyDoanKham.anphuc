using Microsoft.AspNetCore.Authorization;

namespace QuanLyDoanKham.API.Authorization;

public class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public AuthorizePermissionAttribute(string permission)
    {
        Policy = PermissionConstants.PolicyPrefix + permission;
    }
}
