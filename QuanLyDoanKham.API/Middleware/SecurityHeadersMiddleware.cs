namespace QuanLyDoanKham.API.Middleware;

/// <summary>
/// Thêm các security headers vào mọi response để chống XSS, clickjacking, MIME sniffing.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Chống clickjacking
        headers["X-Frame-Options"] = "DENY";

        // Chống MIME sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Chỉ gửi referrer khi cùng origin
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Tắt cache cho API responses (tránh lộ dữ liệu nhạy cảm)
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            headers["Pragma"] = "no-cache";
        }

        // Content Security Policy cơ bản
        headers["X-XSS-Protection"] = "1; mode=block";

        await _next(context);
    }
}
