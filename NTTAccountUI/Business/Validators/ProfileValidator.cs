using System.Text.RegularExpressions;

namespace NTTAccountUI.Business.Validators;

public class ProfileValidator
{
    private readonly List<string> _errors = new();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;
    public ProfileValidator ValidatePassword(string? password, string? confirm)
    {
        if (string.IsNullOrEmpty(password)) return this;
        if (password.Length < 8 || password.Length > 64)
            _errors.Add("Şifre 8-64 karakter arasında olmalıdır.");
        else if (!password.Any(char.IsUpper))
            _errors.Add("Şifre en az bir büyük harf içermelidir.");
        else if (!password.Any(char.IsLower))
            _errors.Add("Şifre en az bir küçük harf içermelidir.");
        else if (!password.Any(char.IsDigit))
            _errors.Add("Şifre en az bir rakam içermelidir.");
        if (password != confirm)
            _errors.Add("Şifreler eşleşmiyor.");
        return this;
    }
    public ProfileValidator ValidateEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return this;
        const string pattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase))
            _errors.Add("Geçerli bir email adresi giriniz.");
        else if (email.Length > 150)
            _errors.Add("Email en fazla 150 karakter olabilir.");
        return this;
    }
}