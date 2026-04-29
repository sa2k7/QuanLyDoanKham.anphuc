using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.API.Services.Auth;

/// <summary>
/// Service to invalidate refresh tokens, forcing users to re-login.
/// 
/// Strategy: Invalidate refresh tokens (set to null) rather than regenerating JWTs server-side.
/// This is the safest approach because:
/// 1. No need to generate tokens for users who may not be active
/// 2. On next login, users get fresh JWTs with correct permissions from the updated DB
/// 3. Existing JWTs remain valid until expiry (typically 1 hour), which is acceptable
/// 4. No risk of generating tokens with wrong data
/// </summary>
public class TokenRefreshService : ITokenRefreshService
{
    private readonly ApplicationDbContext _context;

    public TokenRefreshService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> RefreshAllUserTokensAsync()
    {
        // Invalidate all refresh tokens for active users
        // This forces re-login on next token refresh attempt
        var activeUsers = await _context.Users
            .Where(u => u.IsActive && u.RefreshToken != null)
            .ToListAsync();

        foreach (var user in activeUsers)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
        }

        await _context.SaveChangesAsync();
        return activeUsers.Count;
    }

    /// <inheritdoc />
    public async Task<bool> RefreshUserTokenAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync();
        return true;
    }
}
