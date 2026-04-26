using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NTTAccountUI.Models.ViewModels;
public class UserProfileViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public byte RoleId { get; set; }
    public bool IsActive { get; set; }

    [StringLength(64, MinimumLength = 8, ErrorMessage = "Şifre 8-64 karakter olmalıdır.")]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string? ConfirmPassword { get; set; }

    public string? Note { get; set; }

    public IFormFile? ProfileImageFile { get; set; }
    public string? ProfileImage { get; set; }
}