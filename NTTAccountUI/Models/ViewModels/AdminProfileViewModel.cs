using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

// Admin kendi profilini günceller
public class AdminProfileViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty; // göster ama değiştirme yok (korumalı için)
    public byte RoleId { get; set; }

    // Admin her şeyi değiştirebilir (korumalı kullanıcı hariç)
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    [StringLength(150)]
    public string? NewEmail { get; set; }

    [StringLength(64, MinimumLength = 8, ErrorMessage = "Şifre 8-64 karakter olmalıdır.")]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string? ConfirmPassword { get; set; }

    public string? Note { get; set; }

    public IFormFile? ProfileImageFile { get; set; }
    public string? ProfileImage { get; set; }

    public bool IsProtected { get; set; } // admin@dogukankosan.com ise true
}