using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Data.Repositories;
public interface IContactRepository
{
    Task<bool> CreateAsync(Contact contact);
    Task<bool> HasSpamAsync(string ipAddress, string phone);
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<bool> ToggleReadAsync(int id);
    Task<bool> DeleteAsync(int id);
}
public class ContactRepository : IContactRepository
{
    private readonly DapperContext _context;
    public ContactRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<bool> CreateAsync(Contact contact)
    {
        const string sql = @"
            INSERT INTO Contacts (FullName, Phone, Subject, Message, IpAddress)
            VALUES (@FullName, @Phone, @Subject, @Message, @IpAddress)";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new
        {
            contact.FullName,
            contact.Phone,
            contact.Subject,
            contact.Message,
            contact.IpAddress
        }) > 0;
    }
    public async Task<bool> HasSpamAsync(string ipAddress, string phone)
    {
        const string sql = @"
            SELECT COUNT(1) FROM Contacts
            WHERE IpAddress = @IpAddress
            AND CreatedAt >= DATEADD(HOUR, -1, SYSUTCDATETIME());

            SELECT COUNT(1) FROM Contacts
            WHERE Phone = @Phone
            AND CreatedAt >= DATEADD(HOUR, -24, SYSUTCDATETIME());";
        using var conn = _context.CreateConnection();
        using var multi = await conn.QueryMultipleAsync(sql, new { IpAddress = ipAddress, Phone = phone });
        int ipCount = await multi.ReadFirstAsync<int>();
        int phoneCount = await multi.ReadFirstAsync<int>();
        return ipCount >= 3 || phoneCount >= 5;
    }
    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Contacts ORDER BY CreatedAt DESC";
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<Contact>(sql);
    }
    public async Task<bool> ToggleReadAsync(int id)
    {
        const string sql = @"
            UPDATE Contacts 
            SET IsRead = CASE WHEN IsRead = 1 THEN 0 ELSE 1 END
            WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Contacts WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
}