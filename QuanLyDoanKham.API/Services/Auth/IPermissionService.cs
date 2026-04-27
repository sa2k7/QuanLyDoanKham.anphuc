namespace QuanLyDoanKham.API.Services.Auth;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task<bool> HasPermissionAsync(int userId, string permissionKey);
}
