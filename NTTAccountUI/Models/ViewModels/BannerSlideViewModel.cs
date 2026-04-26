using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

public class BannerSlideViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Başlık 2-100 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Açıklama 5-500 karakter olmalıdır.")]
    public string Description { get; set; } = string.Empty;

    // Yeni görsel yüklenirse
    public IFormFile? ImageFile { get; set; }

    // Mevcut Base64
    public string? Image { get; set; }

    public int OrderNo { get; set; }
    public bool IsActive { get; set; } = true;
}