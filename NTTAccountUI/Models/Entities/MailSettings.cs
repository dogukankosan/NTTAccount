namespace NTTAccountUI.Models.Entities;

public class MailSettings
{
    public int Id { get; set; }
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
    public string? Signature { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}