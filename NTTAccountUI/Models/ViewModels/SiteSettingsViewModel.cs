using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

public class SiteSettingsViewModel
{
    public int Id { get; set; }

    // Zorunlu
    [Required(ErrorMessage = "Site adı zorunludur.")]
    [StringLength(100)]
    public string SiteName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Site açıklaması zorunludur.")]
    [StringLength(500)]
    public string SiteDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Site URL zorunludur.")]
    [StringLength(200)]
    public string SiteUrl { get; set; } = string.Empty;

    // Görseller
    public IFormFile? SiteLogoFile { get; set; }
    public IFormFile? SiteIconFile { get; set; }
    public IFormFile? CeoImageFile { get; set; }

    public string? SiteLogo { get; set; }
    public string? SiteIcon { get; set; }
    public string? CeoImage { get; set; }

    // İletişim
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? WhatsApp { get; set; }

    [StringLength(100)]
    public string? Telegram { get; set; }

    // Sosyal Medya
    [StringLength(200)]
    public string? Facebook { get; set; }

    [StringLength(200)]
    public string? Discord { get; set; }

    [StringLength(200)]
    public string? YouTube { get; set; }

    // CEO
    [StringLength(100)]
    public string? CeoName { get; set; }

    [StringLength(100)]
    public string? CeoTitle { get; set; }

    [StringLength(1000)]
    public string? CeoDescription { get; set; }

    // Hakkında
    [StringLength(2000)]
    public string? AboutText { get; set; }
}