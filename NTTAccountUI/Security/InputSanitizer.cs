using System.Text.RegularExpressions;
using System.Web;

namespace NTTAccountUI.Security;

public static class InputSanitizer
{
    // HTML taglarını ve tehlikeli karakterleri temizler
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        // Boşlukları temizle
        string? sanitized = input.Trim();
        // HTML encode - <script> gibi tagları zararsız hale getirir
        sanitized = HttpUtility.HtmlEncode(sanitized);
        // SQL injection için ekstra önlem - tehlikeli keywordleri temizle
        sanitized = RemoveSqlKeywords(sanitized);
        // Maksimum uzunluk kontrolü
        if (sanitized.Length > 2000)
            sanitized = sanitized[..2000];
        return sanitized;
    }
    // Email için özel sanitize
    public static string SanitizeEmail(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        string? sanitized = input.Trim().ToLowerInvariant();
        // Email'de sadece geçerli karakterlere izin ver
        sanitized = Regex.Replace(sanitized, @"[^a-z0-9@._\-]", string.Empty);
        if (sanitized.Length > 150)
            sanitized = sanitized[..150];
        return sanitized;
    }
    // Username için özel sanitize
    public static string SanitizeUsername(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        string? sanitized = input.Trim();
        // Username'de sadece harf, rakam ve alt çizgiye izin ver
        sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_]", string.Empty);
        if (sanitized.Length > 20)
            sanitized = sanitized[..20];
        return sanitized;
    }
    // Tehlikeli SQL keywordlerini temizle (Dapper zaten koruyor ama ekstra güvenlik)
    private static string RemoveSqlKeywords(string input)
    {
        string[] sqlKeywords =
        [
            "--", ";--", ";", "/*", "*/", "xp_", "EXEC", "EXECUTE",
            "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER",
            "UNION", "SELECT", "CAST", "CONVERT", "CHAR", "NCHAR"
        ];
        foreach (string keyword in sqlKeywords)
        {
            input = Regex.Replace(input, Regex.Escape(keyword), string.Empty, RegexOptions.IgnoreCase);
        }
        return input;
    }
}