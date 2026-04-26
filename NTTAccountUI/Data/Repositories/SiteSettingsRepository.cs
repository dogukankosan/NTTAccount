using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Data.Repositories;
public interface ISiteSettingsRepository
{
    Task<SiteSettings?> GetAsync();
    Task<bool> UpdateAsync(SiteSettings settings, int updatedBy);
}
public class SiteSettingsRepository : ISiteSettingsRepository
{
    private readonly DapperContext _context;
    public SiteSettingsRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<SiteSettings?> GetAsync()
    {
        const string sql = "SELECT TOP 1 * FROM SiteSettings";
        using var conn = _context.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<SiteSettings>(sql);
    }
    public async Task<bool> UpdateAsync(SiteSettings s, int updatedBy)
    {
        const string sql = @"
            UPDATE SiteSettings SET
                SiteName        = @SiteName,
                SiteDescription = @SiteDescription,
                SiteUrl         = @SiteUrl,
                SiteLogo        = @SiteLogo,
                SiteIcon        = @SiteIcon,
                Email           = @Email,
                WhatsApp        = @WhatsApp,
                Telegram        = @Telegram,
                Facebook        = @Facebook,
                Discord         = @Discord,
                YouTube         = @YouTube,
                CeoName         = @CeoName,
                CeoTitle        = @CeoTitle,
                CeoImage        = @CeoImage,
                CeoDescription  = @CeoDescription,
                AboutText       = @AboutText,
                UpdatedAt       = SYSUTCDATETIME(),
                UpdatedBy       = @UpdatedBy
            WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        int rows = await conn.ExecuteAsync(sql, new
        {
            s.Id,
            s.SiteName,
            s.SiteDescription,
            s.SiteUrl,
            s.SiteLogo,
            s.SiteIcon,
            s.Email,
            s.WhatsApp,
            s.Telegram,
            s.Facebook,
            s.Discord,
            s.YouTube,
            s.CeoName,
            s.CeoTitle,
            s.CeoImage,
            s.CeoDescription,
            s.AboutText,
            UpdatedBy = updatedBy
        });
        return rows > 0;
    }
}