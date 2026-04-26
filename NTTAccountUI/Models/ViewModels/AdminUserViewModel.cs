using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

// Admin - kullanıcı ekle/düzenle
public class AdminUserViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
    // Eklemede zorunlu, düzenlemede opsiyonel
    [StringLength(64, MinimumLength = 8, ErrorMessage = "Şifre 8-64 karakter olmalıdır.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Rol zorunludur.")]
    public byte RoleId { get; set; } = 2;
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
    public IFormFile? ProfileImageFile { get; set; }
    public string? ProfileImage { get; set; }
}