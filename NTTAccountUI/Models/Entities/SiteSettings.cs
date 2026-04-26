namespace NTTAccountUI.Models.Entities;

public class SiteSettings
{
    public int Id { get; set; }

    // Zorunlu
    public string SiteName { get; set; } = string.Empty;
    public string SiteDescription { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
    public string SiteLogo { get; set; } = string.Empty;
    public string SiteIcon { get; set; } = string.Empty;

    // İletişim
    public string? Email { get; set; }
    public string? WhatsApp { get; set; }
    public string? Telegram { get; set; }

    // Sosyal Medya
    public string? Facebook { get; set; }
    public string? Discord { get; set; }
    public string? YouTube { get; set; }

    // SEO
    public string? CeoName { get; set; }
    public string? CeoTitle { get; set; }
    public string? CeoImage { get; set; }
    public string? CeoDescription { get; set; }

    // Hakkında
    public string? AboutText { get; set; }

    // Meta
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}