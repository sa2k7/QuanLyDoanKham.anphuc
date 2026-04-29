namespace QuanLyDoanKham.API.Services.Auth;

public interface ITokenRefreshService
{
    /// <summary>
    /// Invalidates all active refresh tokens, forcing all users to re-login.
    /// On next login, users will receive fresh JWTs with correct permissions.
    /// </summary>
    /// <returns>Number of users whose tokens were invalidated</returns>
    Task<int> RefreshAllUserTokensAsync();

    /// <summary>
    /// Invalidates a specific user's refresh token, forcing them to re-login.
    /// </summary>
    /// <param name="userId">The user ID to invalidate token for</param>
    /// <returns>True if the user was found and token invalidated, false otherwise</returns>
    Task<bool> RefreshUserTokenAsync(int userId);
}
