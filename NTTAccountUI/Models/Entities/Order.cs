namespace NTTAccountUI.Models.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserEmail { get; set; }        // JOIN ile gelir
    public string? Note { get; set; }
    public byte Status { get; set; }              // 0=Bekliyor 1=KısmiSevk 2=Kapandı
    public string StatusText => Status switch
    {
        0 => "Bekliyor",
        1 => "Kısmı Sevk",
        2 => "Kapandı",
        _ => "Bilinmiyor"
    };
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}