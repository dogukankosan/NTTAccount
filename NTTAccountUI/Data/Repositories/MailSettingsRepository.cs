using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Security;

namespace NTTAccountUI.Data.Repositories;
public interface IMailSettingsRepository
{
    Task<MailSettings?> GetAsync();
    Task<bool> UpdateAsync(MailSettings settings, int updatedBy);
}
public class MailSettingsRepository : IMailSettingsRepository
{
    private readonly DapperContext _context;
    private readonly IEncryptionService _encryption;
    public MailSettingsRepository(DapperContext context, IEncryptionService encryption)
    {
        _context = context;
        _encryption = encryption;
    }
    public async Task<MailSettings?> GetAsync()
    {
        const string sql = "SELECT TOP 1 * FROM MailSettings ORDER BY Id ASC";
        using var conn = _context.CreateConnection();
        MailSettings result = await conn.QueryFirstOrDefaultAsync<MailSettings>(sql);
        // Şifreyi çöz
        if (result != null && !string.IsNullOrEmpty(result.SmtpPassword))
        {
            try { result.SmtpPassword = _encryption.Decrypt(result.SmtpPassword); }
            catch { /* Eski plain text kayıt varsa olduğu gibi bırak */ }
        }
        return result;
    }
    public async Task<bool> UpdateAsync(MailSettings settings, int updatedBy)
    {
        // Şifreyi şifrele
        string? encryptedPassword = string.IsNullOrEmpty(settings.SmtpPassword)
            ? string.Empty
            : _encryption.Encrypt(settings.SmtpPassword);
        const string sql = @"
            IF EXISTS (SELECT 1 FROM MailSettings WHERE Id = @Id)
                UPDATE MailSettings SET
                    SmtpHost     = @SmtpHost,
                    SmtpPort     = @SmtpPort,
                    SmtpUser     = @SmtpUser,
                    SmtpPassword = CASE WHEN @SmtpPassword = '' THEN SmtpPassword ELSE @SmtpPassword END,
                    FromName     = @FromName,
                    FromEmail    = @FromEmail,
                    UseSsl       = @UseSsl,
                    Signature    = @Signature,
                    IsActive     = 1,
                    UpdatedAt    = SYSUTCDATETIME(),
                    UpdatedBy    = @UpdatedBy
                WHERE Id = @Id
            ELSE
                INSERT INTO MailSettings (SmtpHost, SmtpPort, SmtpUser, SmtpPassword, FromName, FromEmail, UseSsl, Signature, IsActive, UpdatedBy)
                VALUES (@SmtpHost, @SmtpPort, @SmtpUser, @SmtpPassword, @FromName, @FromEmail, @UseSsl, @Signature, 1, @UpdatedBy)";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new
        {
            settings.Id,
            settings.SmtpHost,
            settings.SmtpPort,
            settings.SmtpUser,
            SmtpPassword = encryptedPassword,
            settings.FromName,
            settings.FromEmail,
            settings.UseSsl,
            settings.Signature,
            UpdatedBy = updatedBy
        }) > 0;
    }
}