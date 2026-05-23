namespace NTTAccountUI.Models.Entities;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByEmail { get; set; }  // JOIN ile gelecek
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}