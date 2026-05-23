namespace NTTAccountUI.Models.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }      // JOIN ile gelir
    public string? ProductCode { get; set; }      // JOIN ile gelir
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterPw { get; set; } = string.Empty;
    public string CharacterMail { get; set; } = string.Empty;
    public string CharacterMailPw { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string OtpPassword { get; set; } = string.Empty;
    public string ZipFile { get; set; } = string.Empty;  // base64
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}