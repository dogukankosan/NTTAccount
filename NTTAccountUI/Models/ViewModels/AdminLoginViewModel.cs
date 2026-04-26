using System.ComponentModel.DataAnnotations;

namespace NTTAccountUI.Models.ViewModels;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}