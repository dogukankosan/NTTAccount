using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

public class OrderViewModel
{
    public int Id { get; set; }
    public string? OrderNo { get; set; }
    public int UserId { get; set; }
    public string? Note { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UserEmail { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Ürün zorunludur.")]
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    [Required(ErrorMessage = "Miktar zorunludur.")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    public int Quantity { get; set; }
    public string? UnitPriceRaw { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal FinalUnitPrice =>
        decimal.TryParse(UnitPriceRaw,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var result) ? result : UnitPrice;

    [Required(ErrorMessage = "Server adı zorunludur.")]
    [StringLength(200)]
    public string ServerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Karakter ID zorunludur.")]
    [StringLength(200)]
    public string CharacterId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Karakter şifresi zorunludur.")]
    [StringLength(200)]
    public string CharacterPw { get; set; } = string.Empty;

    [Required(ErrorMessage = "Karakter maili zorunludur.")]
    [StringLength(200)]
    public string CharacterMail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mail şifresi zorunludur.")]
    [StringLength(200)]
    public string CharacterMailPw { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP kodu zorunludur.")]
    [StringLength(200)]
    public string OtpCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP şifresi zorunludur.")]
    [StringLength(200)]
    public string OtpPassword { get; set; } = string.Empty;
    public string? ZipFile { get; set; }
    public string? ZipBase64 { get; set; }
    public string? ZipName { get; set; }
    public IFormFile? ZipFileUpload { get; set; }
    public bool IsClosed { get; set; }
}