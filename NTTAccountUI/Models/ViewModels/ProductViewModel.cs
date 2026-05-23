using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;
public class ProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün kodu zorunludur.")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stok 0 veya üzeri olmalıdır.")]
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
}