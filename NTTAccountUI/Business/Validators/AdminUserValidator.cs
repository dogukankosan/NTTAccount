using System.Text.RegularExpressions;

namespace NTTAccountUI.Business.Validators;

// Admin kullanıcı ekle/düzenle validator
public class AdminUserValidator
{
    private readonly List<string> _errors = new();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;
    public AdminUserValidator Validate(string? email, string? password, byte roleId, bool isNew)
    {
        ValidateEmail(email);
        if (isNew || !string.IsNullOrEmpty(password))
            ValidatePassword(password);
        ValidateRole(roleId);
        return this;
    }
    private void ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _errors.Add("Email zorunludur.");
            return;
        }
        const string pattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase))
            _errors.Add("Geçerli bir email adresi giriniz.");
        else if (email.Length > 150)
            _errors.Add("Email en fazla 150 karakter olabilir.");
    }
    private void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            _errors.Add("Şifre zorunludur.");
            return;
        }
        if (password.Length < 8 || password.Length > 64)
        {
            _errors.Add("Şifre 8-64 karakter arasında olmalıdır.");
            return;
        }
        if (!password.Any(char.IsUpper))
            _errors.Add("Şifre en az bir büyük harf içermelidir.");
        if (!password.Any(char.IsLower))
            _errors.Add("Şifre en az bir küçük harf içermelidir.");
        if (!password.Any(char.IsDigit))
            _errors.Add("Şifre en az bir rakam içermelidir.");
    }
    private void ValidateRole(byte roleId)
    {
        if (roleId != 1 && roleId != 2)
            _errors.Add("Geçerli bir rol seçiniz.");
    }
}
// User kendi profilini düzenle validator
public class UserProfileValidator
{
    private readonly List<string> _errors = new();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;
    public UserProfileValidator Validate(string? newPassword, string? confirmPassword)
    {
        if (!string.IsNullOrEmpty(newPassword))
            ValidatePassword(newPassword, confirmPassword);
        return this;
    }
    private void ValidatePassword(string? password, string? confirm)
    {
        if (password!.Length < 8 || password.Length > 64)
        {
            _errors.Add("Şifre 8-64 karakter arasında olmalıdır.");
            return;
        }
        if (!password.Any(char.IsUpper))
            _errors.Add("Şifre en az bir büyük harf içermelidir.");
        if (!password.Any(char.IsLower))
            _errors.Add("Şifre en az bir küçük harf içermelidir.");
        if (!password.Any(char.IsDigit))
            _errors.Add("Şifre en az bir rakam içermelidir.");
        if (password != confirm)
            _errors.Add("Şifreler eşleşmiyor.");
    }
}