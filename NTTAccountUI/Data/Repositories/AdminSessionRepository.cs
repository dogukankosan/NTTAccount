using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Data.Repositories;
public interface IAdminSessionRepository
{
    Task<string> CreateSessionAsync(int userId, string ipAddress, string? userAgent, int expireHours = 8);
    Task<AdminSession?> GetValidSessionAsync(string token);
    Task RevokeSessionAsync(string token);
    Task RevokeAllSessionsAsync(int userId);
    Task CleanExpiredSessionsAsync();
}
public class AdminSessionRepository : IAdminSessionRepository
{
    private readonly DapperContext _context;
    public AdminSessionRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<string> CreateSessionAsync(int userId, string ipAddress, string? userAgent, int expireHours = 8)
    {
        string? token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
        const string sql = @"
            INSERT INTO AdminSessions (UserId, SessionToken, IpAddress, UserAgent, ExpiresAt)
            VALUES (@UserId, @Token, @IpAddress, @UserAgent, @ExpiresAt)";
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            UserId = userId,
            Token = token,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = DateTime.UtcNow.AddHours(expireHours)
        });
        return token;
    }
    public async Task<AdminSession?> GetValidSessionAsync(string token)
    {
        // RoleId kontrolü yok artık — hem Admin hem User girebilir
        // Middleware role bazlı kısıtlamayı kendisi yapıyor
        const string sql = @"
            SELECT s.Id, s.UserId, s.SessionToken, s.IpAddress, s.ExpiresAt, s.IsRevoked,
                   u.Email, u.RoleId, u.IsActive
            FROM AdminSessions s
            INNER JOIN Users u ON u.Id = s.UserId
            WHERE s.SessionToken = @Token
              AND s.IsRevoked    = 0
              AND s.ExpiresAt    > SYSUTCDATETIME()
              AND u.IsActive     = 1";
        using var conn = _context.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<AdminSession>(sql, new { Token = token });
    }
    public async Task RevokeSessionAsync(string token)
    {
        const string sql = "UPDATE AdminSessions SET IsRevoked = 1 WHERE SessionToken = @Token";
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync(sql, new { Token = token });
    }
    public async Task RevokeAllSessionsAsync(int userId)
    {
        const string sql = "UPDATE AdminSessions SET IsRevoked = 1 WHERE UserId = @UserId";
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync(sql, new { UserId = userId });
    }
    public async Task CleanExpiredSessionsAsync()
    {
        const string sql = "DELETE FROM AdminSessions WHERE ExpiresAt < SYSUTCDATETIME()";
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync(sql);
    }
}