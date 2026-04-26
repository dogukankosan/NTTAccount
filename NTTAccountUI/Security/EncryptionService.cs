using System.Security.Cryptography;
using System.Text;

namespace NTTAccountUI.Security;
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    public EncryptionService(IConfiguration configuration)
    {
        string? keyString = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key appsettings'de tanımlı değil.");
        // Key'i tam 32 byte (256-bit) yap
        _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
    }  
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV(); // Her şifrelemede farklı IV
        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        // IV + şifreli veri → Base64
        byte[] result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
        return Convert.ToBase64String(result);
    }
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        byte[] fullBytes = Convert.FromBase64String(cipherText);
        using Aes aes = Aes.Create();
        aes.Key = _key;
        // İlk 16 byte IV, geri kalanı şifreli veri
        byte[] iv = new byte[16];
        byte[] encrypted = new byte[fullBytes.Length - 16];
        Buffer.BlockCopy(fullBytes, 0, iv, 0, 16);
        Buffer.BlockCopy(fullBytes, 16, encrypted, 0, encrypted.Length);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
        return Encoding.UTF8.GetString(decrypted);
    }
}