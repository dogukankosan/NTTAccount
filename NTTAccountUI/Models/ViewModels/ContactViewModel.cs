using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

public class ContactViewModel
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad Soyad 2-100 karakter olmalıdır.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konu zorunludur.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Konu 3-200 karakter olmalıdır.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mesaj zorunludur.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mesaj 10-2000 karakter olmalıdır.")]
    public string Message { get; set; } = string.Empty;

    // Honeypot
    public string? Website { get; set; }
}