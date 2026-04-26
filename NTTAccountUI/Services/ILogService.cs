namespace NTTAccountUI.Services;

public interface ILogService
{
    Task LogErrorAsync(Exception ex, string? source = null, int? userId = null);
    Task LogWarningAsync(string message, string? source = null, int? userId = null);
    Task LogInfoAsync(string message, string? source = null, int? userId = null);
    Task LogCriticalAsync(Exception ex, string? source = null, int? userId = null);
}