namespace NTTAccountUI.Business.Validators;

public class BannerSlideValidator
{
    private readonly List<string> _errors = new();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;
    public BannerSlideValidator Validate(string? title, string? description, string? image, bool isNew)
    {
        ValidateTitle(title);
        ValidateDescription(description);
        // Yeni kayıtta görsel zorunlu, güncellemede değil
        if (isNew) ValidateImage(image);
        return this;
    }
    private void ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            _errors.Add("Başlık zorunludur.");
            return;
        }
        if (title.Length < 2 || title.Length > 100)
            _errors.Add("Başlık 2-100 karakter arasında olmalıdır.");
    }
    private void ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            _errors.Add("Açıklama zorunludur.");
            return;
        }
        if (description.Length < 5 || description.Length > 500)
            _errors.Add("Açıklama 5-500 karakter arasında olmalıdır.");
    }
    private void ValidateImage(string? image)
    {
        if (string.IsNullOrWhiteSpace(image))
            _errors.Add("Görsel zorunludur.");
    }
}