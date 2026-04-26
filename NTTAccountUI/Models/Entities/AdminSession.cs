namespace NTTAccountUI.Models.Entities;

public class AdminSession
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    // Join'den gelen user bilgileri
    public string Email { get; set; } = string.Empty;
    public byte RoleId { get; set; }
    public bool IsActive { get; set; }
}