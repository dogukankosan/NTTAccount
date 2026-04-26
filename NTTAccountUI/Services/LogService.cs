using Dapper;
using NTTAccountUI.Data;

namespace NTTAccountUI.Services;

public class LogService : ILogService
{
    private readonly DapperContext _context;
    private readonly IHttpContextAccessor _httpContext;
    public LogService(DapperContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
    }
    public async Task LogErrorAsync(Exception ex, string? source = null, int? userId = null)
        => await InsertAsync("Error", ex.Message, ex.ToString(), source, userId);
    public async Task LogWarningAsync(string message, string? source = null, int? userId = null)
        => await InsertAsync("Warning", message, null, source, userId);
    public async Task LogInfoAsync(string message, string? source = null, int? userId = null)
        => await InsertAsync("Info", message, null, source, userId);
    public async Task LogCriticalAsync(Exception ex, string? source = null, int? userId = null)
        => await InsertAsync("Critical", ex.Message, ex.ToString(), source, userId);
    private async Task InsertAsync(string level, string message, string? exception, string? source, int? userId)
    {
        try
        {
            var http = _httpContext.HttpContext;
            const string sql = @"
                INSERT INTO ErrorLogs 
                    (Level, Message, Exception, Source, RequestPath, RequestMethod, UserId, IpAddress, UserAgent)
                VALUES 
                    (@Level, @Message, @Exception, @Source, @RequestPath, @RequestMethod, @UserId, @IpAddress, @UserAgent)";
            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(sql, new
            {
                Level = level,
                Message = message.Length > 2000 ? message[..2000] : message,
                Exception = exception,
                Source = source,
                RequestPath = http?.Request.Path.Value,
                RequestMethod = http?.Request.Method,
                UserId = userId,
                IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = http?.Request.Headers["User-Agent"].ToString()
            });
        }
        catch
        {
            // Loglama kendisi patlarsa uygulamayı çökertme
        }
    }
}
