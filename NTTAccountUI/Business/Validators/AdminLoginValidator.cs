using System.Text.RegularExpressions;

namespace NTTAccountUI.Business.Validators;

public class AdminLoginValidator
{
    private readonly List<string> _errors = new();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;
    public AdminLoginValidator Validate(string? email, string? password)
    {
        ValidateEmail(email);
        ValidatePassword(password);
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
    }
    private void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            _errors.Add("Şifre zorunludur.");
    }
}