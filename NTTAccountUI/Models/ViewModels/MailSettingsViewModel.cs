using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

public class MailSettingsViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "SMTP sunucu adresi zorunludur.")]
    [StringLength(200)]
    public string SmtpHost { get; set; } = string.Empty;

    [Required(ErrorMessage = "Port zorunludur.")]
    [Range(1, 65535, ErrorMessage = "Geçerli bir port giriniz.")]
    public int SmtpPort { get; set; } = 587;

    [Required(ErrorMessage = "SMTP kullanıcı adı zorunludur.")]
    [StringLength(200)]
    public string SmtpUser { get; set; } = string.Empty;

    // Boş bırakılırsa mevcut şifre korunur
    [StringLength(500)]
    public string? SmtpPassword { get; set; }

    [Required(ErrorMessage = "Gönderici adı zorunludur.")]
    [StringLength(100)]
    public string FromName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gönderici email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    [StringLength(150)]
    public string FromEmail { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = true;

    // HTML imza
    public string? Signature { get; set; }

    public bool IsActive { get; set; } = true;

    // Test maili gönderilecek adres
    [EmailAddress(ErrorMessage = "Geçerli bir test email adresi giriniz.")]
    public string? TestEmail { get; set; }

    // Test başarılı mı? (session'da tutacağız)
    public bool TestPassed { get; set; }
}