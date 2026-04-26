using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Data.Repositories;

public interface IErrorLogRepository
{
    Task<IEnumerable<ErrorLog>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllAsync();
}

public class ErrorLogRepository : IErrorLogRepository
{
    private readonly DapperContext _context;
    public ErrorLogRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<ErrorLog>> GetAllAsync()
    {
        const string sql = "SELECT * FROM ErrorLogs ORDER BY CreatedAt DESC";
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<ErrorLog>(sql);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM ErrorLogs WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
    public async Task<bool> DeleteAllAsync()
    {
        const string sql = "DELETE FROM ErrorLogs";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql) > 0;
    }
}