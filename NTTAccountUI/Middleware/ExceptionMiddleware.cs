using NTTAccountUI.Services;

namespace NTTAccountUI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    public ExceptionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
            string source = $"{context.Request.Path}";
            await logService.LogCriticalAsync(ex, source);
            // Kullanıcıyı hata sayfasına yönlendir
            context.Response.Redirect("/Home/Error");
        }
    }
}