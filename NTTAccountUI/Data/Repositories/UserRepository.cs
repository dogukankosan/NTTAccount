using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<bool> AdminUpdatePasswordAsync(int userId, string passwordHash);
    Task UpdateLoginInfoAsync(int userId, string ipAddress, bool isSuccess);

    // Admin
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> CreateAsync(User user, string passwordHash);
    Task<bool> AdminUpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int excludeId = 0);
    // User kendi profili
    Task<bool> UserUpdateAsync(int userId, string? passwordHash, string? note, string? profileImage);
}
public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;
    public UserRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT u.Id, u.Email, u.PasswordHash,
                   u.RoleId, r.Name AS RoleName, u.IsActive,
                   u.LoginFailCount, u.LockoutUntil, u.ProfileImage, u.Note
            FROM Users u
            INNER JOIN Roles r ON r.Id = u.RoleId
            WHERE u.Email = @Email";
        using var conn = _context.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }
    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT u.Id, u.Email, u.PasswordHash, u.RoleId, u.IsActive,
                   u.ProfileImage, u.Note, u.CreatedAt, u.LastLoginAt, u.LastLoginIp
            FROM Users u
            WHERE u.Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = @"
            SELECT u.Id, u.Email, u.RoleId, r.Name AS RoleName,
                   u.IsActive, u.ProfileImage, u.Note,
                   u.CreatedAt, u.LastLoginAt, u.LastLoginIp, u.LoginFailCount
            FROM Users u
            INNER JOIN Roles r ON r.Id = u.RoleId
            ORDER BY u.CreatedAt DESC";
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<User>(sql);
    }
    public async Task<int> CreateAsync(User user, string passwordHash)
    {
        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, RoleId, IsActive, ProfileImage, Note)
            VALUES (@Email, @PasswordHash, @RoleId, @IsActive, @ProfileImage, @Note);
            SELECT SCOPE_IDENTITY();";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            user.Email,
            PasswordHash = passwordHash,
            user.RoleId,
            user.IsActive,
            user.ProfileImage,
            user.Note
        });
    }
    public async Task<bool> AdminUpdateAsync(User user)
    {
        const string sql = @"
            UPDATE Users SET
                Email        = @Email,
                RoleId       = @RoleId,
                IsActive     = @IsActive,
                Note         = @Note,
                ProfileImage = CASE WHEN @ProfileImage = '' THEN ProfileImage ELSE @ProfileImage END,
                UpdatedAt    = SYSUTCDATETIME()
            WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new
        {
            user.Id,
            user.Email,
            user.RoleId,
            user.IsActive,
            user.Note,
            ProfileImage = user.ProfileImage ?? string.Empty
        }) > 0;
    }
    public async Task<bool> AdminUpdatePasswordAsync(int userId, string passwordHash)
    {
        const string sql = @"
            UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash }) > 0;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
    public async Task<bool> EmailExistsAsync(string email, int excludeId = 0)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND Id != @ExcludeId";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new { Email = email, ExcludeId = excludeId }) > 0;
    }
    public async Task<bool> UserUpdateAsync(int userId, string? passwordHash, string? note, string? profileImage)
    {
        const string sql = @"
            UPDATE Users SET
                PasswordHash = CASE WHEN @PasswordHash IS NULL THEN PasswordHash ELSE @PasswordHash END,
                Note         = @Note,
                ProfileImage = CASE WHEN @ProfileImage IS NULL THEN ProfileImage ELSE @ProfileImage END,
                UpdatedAt    = SYSUTCDATETIME()
            WHERE Id = @UserId";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new
        {
            UserId = userId,
            PasswordHash = passwordHash,
            Note = note,
            ProfileImage = profileImage
        }) > 0;
    }
    public async Task UpdateLoginInfoAsync(int userId, string ipAddress, bool isSuccess)
    {
        using var conn = _context.CreateConnection();
        if (isSuccess)
        {
            const string sql = @"
                UPDATE Users SET LastLoginAt = SYSUTCDATETIME(), LastLoginIp = @IpAddress,
                    LoginFailCount = 0, LockoutUntil = NULL WHERE Id = @UserId";
            await conn.ExecuteAsync(sql, new { UserId = userId, IpAddress = ipAddress });
        }
        else
        {
            const string sql = @"
                UPDATE Users SET LoginFailCount = LoginFailCount + 1,
                    LockoutUntil = CASE WHEN LoginFailCount + 1 >= 5
                        THEN DATEADD(MINUTE, 15, SYSUTCDATETIME()) ELSE LockoutUntil END
                WHERE Id = @UserId";
            await conn.ExecuteAsync(sql, new { UserId = userId, IpAddress = ipAddress });
        }
    }
}