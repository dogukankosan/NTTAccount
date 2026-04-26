using System.Text.RegularExpressions;

namespace NTTAccountUI.Business.Validators;

public class ContactValidator
{
    private readonly List<string> _errors = new();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;
    public ContactValidator Validate(string? fullName, string? phone, string? subject, string? message)
    {
        ValidateFullName(fullName);
        ValidatePhone(phone);
        ValidateSubject(subject);
        ValidateMessage(message);
        return this;
    }
    private void ValidateFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            _errors.Add("Ad Soyad zorunludur.");
            return;
        }
        if (fullName.Length < 2 || fullName.Length > 100)
        {
            _errors.Add("Ad Soyad 2-100 karakter arasında olmalıdır.");
            return;
        }
        const string pattern = @"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$";
        if (!Regex.IsMatch(fullName.Trim(), pattern))
            _errors.Add("Ad Soyad sadece harf içermelidir.");
    }
    private void ValidatePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            _errors.Add("Telefon zorunludur.");
            return;
        }
        if (phone.Length > 20)
        {
            _errors.Add("Telefon en fazla 20 karakter olabilir.");
            return;
        }
        const string pattern = @"^\+?[0-9\s\-\(\)]{7,20}$";
        if (!Regex.IsMatch(phone.Trim(), pattern))
            _errors.Add("Geçerli bir telefon numarası giriniz.");
    }
    private void ValidateSubject(string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            _errors.Add("Konu zorunludur.");
            return;
        }
        if (subject.Length < 3 || subject.Length > 200)
            _errors.Add("Konu 3-200 karakter arasında olmalıdır.");
    }
    private void ValidateMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _errors.Add("Mesaj zorunludur.");
            return;
        }
        if (message.Length < 10 || message.Length > 2000)
            _errors.Add("Mesaj 10-2000 karakter arasında olmalıdır.");
    }
}