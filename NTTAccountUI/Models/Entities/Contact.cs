namespace NTTAccountUI.Models.Entities;

public class Contact
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}