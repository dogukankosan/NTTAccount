using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Middleware;

public class AdminAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    // Sadece Admin (RoleId=1) erişebilir
    private static readonly string[] AdminOnlyPaths =
    [
        "/adminuser",
        "/admincontact",
        "/adminlog",
        "/adminsitesettings",
        "/adminbannerslide",
        "/adminmailsettings"
    ];
    // YENİ:
    private static readonly string[] AuthPaths =
    [
        "/adminhome",
    "/profile"
    ];
    // Kontrol dışı
    private static readonly string[] ExcludedPaths =
    [
        "/adminlogin",
        "/adminlogin/index",
        "/adminlogin/logout" ,
        "/error"  // bunu ekle
    ];
    public AdminAuthMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        string? path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        // Excluded path → direkt geç
        if (ExcludedPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }
        bool isAdminOnlyPath = AdminOnlyPaths.Any(p => path.StartsWith(p));
        bool isAuthPath = AuthPaths.Any(p => path.StartsWith(p));
       // Admin veya auth path değilse geç
        if (!isAdminOnlyPath && !isAuthPath)
        {
            await _next(context);
            return;
        }
        string? token = context.Request.Cookies["AdminSession"];
        if (string.IsNullOrEmpty(token))
        {
            context.Response.Redirect("/AdminLogin");
            return;
        }
        using var scope = _scopeFactory.CreateScope();
        var sessionRepo = scope.ServiceProvider.GetRequiredService<IAdminSessionRepository>();
        AdminSession? session = await sessionRepo.GetValidSessionAsync(token);
        if (session == null)
        {
            context.Response.Cookies.Delete("AdminSession");
            context.Response.Redirect("/AdminLogin");
            return;
        }
        // IP kontrolü
        string currentIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? string.Empty;
        // Birden fazla IP varsa ilkini al
        if (currentIp.Contains(','))
            currentIp = currentIp.Split(',')[0].Trim();
        if (!string.IsNullOrEmpty(currentIp) && session.IpAddress != currentIp)
        {
            await sessionRepo.RevokeSessionAsync(token);
            context.Response.Cookies.Delete("AdminSession");
            context.Response.Redirect("/AdminLogin");
            return;
        }
        // Admin only path → RoleId=1 değilse 403
        if (isAdminOnlyPath && session.RoleId != 1)
        {
            context.Response.StatusCode = 403;
            context.Response.Redirect("/AdminHome");
            return;
        }
        context.Items["AdminUserId"] = session.UserId;
        context.Items["AdminEmail"] = session.Email;
        context.Items["AdminRoleId"] = session.RoleId;
        await _next(context);
    }
}