using System.Text.RegularExpressions;

namespace NTTAccountUI.Business.Validators;

public class SiteSettingsValidator
{
    private readonly List<string> _errors = new();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;  
    public SiteSettingsValidator Validate(
        string? siteName,
        string? siteDescription,
        string? siteUrl,
        string? siteLogo,
        string? siteIcon,
        string? email = null,
        string? whatsApp = null,
        string? telegram = null,
        string? facebook = null,
        string? discord = null,
        string? youTube = null,
        string? ceoName = null,
        string? ceoTitle = null,
        string? ceoDescription = null,
        string? aboutText = null)
    {
        // Zorunlu alanlar
        ValidateSiteName(siteName);
        ValidateSiteDescription(siteDescription);
        ValidateSiteUrl(siteUrl);
        ValidateSiteLogo(siteLogo);
        ValidateSiteIcon(siteIcon);
        // Opsiyonel alanlar — değer varsa kontrol et
        if (!string.IsNullOrWhiteSpace(email)) ValidateEmail(email);
        if (!string.IsNullOrWhiteSpace(whatsApp)) ValidateWhatsApp(whatsApp);
        if (!string.IsNullOrWhiteSpace(telegram)) ValidateTelegram(telegram);
        if (!string.IsNullOrWhiteSpace(facebook)) ValidateUrl("Facebook", facebook);
        if (!string.IsNullOrWhiteSpace(discord)) ValidateUrl("Discord", discord);
        if (!string.IsNullOrWhiteSpace(youTube)) ValidateUrl("YouTube", youTube);
        if (!string.IsNullOrWhiteSpace(ceoName)) ValidateCeoName(ceoName);
        if (!string.IsNullOrWhiteSpace(ceoTitle)) ValidateCeoTitle(ceoTitle);
        if (!string.IsNullOrWhiteSpace(ceoDescription)) ValidateCeoDescription(ceoDescription);
        if (!string.IsNullOrWhiteSpace(aboutText)) ValidateAboutText(aboutText);
        return this;
    }
    // ── Zorunlu ──────────────────────────────────────────
    private void ValidateSiteName(string? siteName)
    {
        if (string.IsNullOrWhiteSpace(siteName))
        {
            _errors.Add("Site adı zorunludur.");
            return;
        }
        if (siteName.Length < 2 || siteName.Length > 100)
            _errors.Add("Site adı 2-100 karakter arasında olmalıdır.");
    }
    private void ValidateSiteDescription(string? siteDescription)
    {
        if (string.IsNullOrWhiteSpace(siteDescription))
        {
            _errors.Add("Site açıklaması zorunludur.");
            return;
        }
        if (siteDescription.Length < 5 || siteDescription.Length > 500)
            _errors.Add("Site açıklaması 5-500 karakter arasında olmalıdır.");
    }
    private void ValidateSiteUrl(string? siteUrl)
    {
        if (string.IsNullOrWhiteSpace(siteUrl))
        {
            _errors.Add("Site URL zorunludur.");
            return;
        }
        if (!Uri.TryCreate(siteUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            _errors.Add("Geçerli bir site URL giriniz. (https://...)");
    }
    private void ValidateSiteLogo(string? siteLogo)
    {
        if (string.IsNullOrWhiteSpace(siteLogo))
            _errors.Add("Site logosu zorunludur.");
    }
    private void ValidateSiteIcon(string? siteIcon)
    {
        if (string.IsNullOrWhiteSpace(siteIcon))
            _errors.Add("Site ikonu zorunludur.");
    }
    // ── Opsiyonel ────────────────────────────────────────
    private void ValidateEmail(string email)
    {
        const string pattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase))
            _errors.Add("Geçerli bir email adresi giriniz.");
        else if (email.Length > 150)
            _errors.Add("Email en fazla 150 karakter olabilir.");
    }
    private void ValidateWhatsApp(string whatsApp)
    {
        // +905xxxxxxxxx formatı
        const string pattern = @"^\+?[0-9]{10,15}$";
        if (!Regex.IsMatch(whatsApp.Trim(), pattern))
            _errors.Add("Geçerli bir WhatsApp numarası giriniz. (+905xxxxxxxxx)");
    }
    private void ValidateTelegram(string telegram)
    {
        if (telegram.Length > 100)
            _errors.Add("Telegram 100 karakterden fazla olamaz.");
    }
    private void ValidateUrl(string fieldName, string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            _errors.Add($"Geçerli bir {fieldName} URL giriniz. (https://...)");
        else if (url.Length > 200)
            _errors.Add($"{fieldName} URL en fazla 200 karakter olabilir.");
    }
    private void ValidateCeoName(string ceoName)
    {
        if (ceoName.Length < 2 || ceoName.Length > 100)
            _errors.Add("SEO adı 2-100 karakter arasında olmalıdır.");
    }
    private void ValidateCeoTitle(string ceoTitle)
    {
        if (ceoTitle.Length < 2 || ceoTitle.Length > 100)
            _errors.Add("SEO unvanı 2-100 karakter arasında olmalıdır.");
    }
    private void ValidateCeoDescription(string ceoDescription)
    {
        if (ceoDescription.Length > 1000)
            _errors.Add("SEO açıklaması en fazla 1000 karakter olabilir.");
    }
    private void ValidateAboutText(string aboutText)
    {
        if (aboutText.Length > 2000)
            _errors.Add("Hakkında yazısı en fazla 2000 karakter olabilir.");
    }
}