using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Data.Repositories;
public interface IBannerSlideRepository
{
    Task<IEnumerable<BannerSlide>> GetAllAsync();
    Task<IEnumerable<BannerSlide>> GetActiveAsync();
    Task<BannerSlide?> GetByIdAsync(int id);
    Task<int> CreateAsync(BannerSlide slide);
    Task<bool> UpdateAsync(BannerSlide slide);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleActiveAsync(int id);
}
public class BannerSlideRepository : IBannerSlideRepository
{
    private readonly DapperContext _context;
    public BannerSlideRepository(DapperContext context)
    {
        _context = context;
    }
    // Admin - tümü
    public async Task<IEnumerable<BannerSlide>> GetAllAsync()
    {
        const string sql = "SELECT * FROM BannerSlides ORDER BY OrderNo ASC, Id ASC";
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<BannerSlide>(sql);
    }
    // Ana sayfa - sadece aktifler
    public async Task<IEnumerable<BannerSlide>> GetActiveAsync()
    {
        const string sql = "SELECT * FROM BannerSlides WHERE IsActive = 1 ORDER BY OrderNo ASC, Id ASC";
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<BannerSlide>(sql);
    }
    public async Task<BannerSlide?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM BannerSlides WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<BannerSlide>(sql, new { Id = id });
    }
    public async Task<int> CreateAsync(BannerSlide slide)
    {
        const string sql = @"
            INSERT INTO BannerSlides (Title, Description, Image, OrderNo, IsActive)
            VALUES (@Title, @Description, @Image, @OrderNo, @IsActive);
            SELECT SCOPE_IDENTITY();";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            slide.Title,
            slide.Description,
            slide.Image,
            slide.OrderNo,
            slide.IsActive
        });
    }
    public async Task<bool> UpdateAsync(BannerSlide slide)
    {
        const string sql = @"
            UPDATE BannerSlides SET
                Title       = @Title,
                Description = @Description,
                Image       = CASE WHEN @Image = '' THEN Image ELSE @Image END,
                OrderNo     = @OrderNo,
                IsActive    = @IsActive,
                UpdatedAt   = SYSUTCDATETIME()
            WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new
        {
            slide.Id,
            slide.Title,
            slide.Description,
            Image = slide.Image ?? string.Empty,
            slide.OrderNo,
            slide.IsActive
        }) > 0;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM BannerSlides WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
    public async Task<bool> ToggleActiveAsync(int id)
    {
        const string sql = @"
            UPDATE BannerSlides 
            SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
}