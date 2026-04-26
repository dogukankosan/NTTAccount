using BCrypt.Net;

namespace NTTAccountUI.Security;

public static class PasswordHasher
{
    // Work factor - ne kadar yüksek o kadar güvenli ama o kadar yavaş
    // 12 production için ideal denge
    private const int WorkFactor = 12;

    // Şifreyi hashle
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }
    // Şifreyi doğrula
    public static bool Verify(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
    // Hashin yenilenmesi gerekiyor mu? (work factor değiştiyse)
    public static bool NeedsRehash(string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
        }
        catch
        {
            return false;
        }
    }
}