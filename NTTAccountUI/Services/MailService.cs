using System.Net;
using System.Net.Mail;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Services;
public interface IMailService
{
    Task<(bool Success, string Error)> SendTestAsync(string smtpHost, int smtpPort, string smtpUser, string smtpPassword, string fromName, string fromEmail, bool useSsl, string toEmail, string? signature = null);
    Task<(bool Success, string Error)> SendAsync(string toEmail, string subject, string body);
}
public class MailService : IMailService
{
    private readonly IMailSettingsRepository _mailRepo;
    public MailService(IMailSettingsRepository mailRepo)
    {
        _mailRepo = mailRepo;
    }
    public async Task<(bool Success, string Error)> SendTestAsync(
        string smtpHost, int smtpPort, string smtpUser, string smtpPassword,
        string fromName, string fromEmail, bool useSsl, string toEmail, string? signature = null)
    {
        try
        {
            // İmza varsa ekle
            string body = @"
                <div style='font-family:Arial,sans-serif;font-size:14px;color:#333;'>
                    <h2 style='color:#e84545;'>Test Maili</h2>
                    <p>SMTP ayarlariniz basariyla dogrulandi. Bu mail otomatik olarak gonderilmistir.</p>
                    <p style='color:#888;font-size:12px;'>Gonderen: " + fromEmail + @"</p>
                </div>";
            if (!string.IsNullOrEmpty(signature))
                body += "<br/><hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>" + signature;
            using SmtpClient client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = useSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000
            };
            MailMessage mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "NTT Hesap - SMTP Test Maili",
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);
            await client.SendMailAsync(mail);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    public async Task<(bool Success, string Error)> SendAsync(string toEmail, string subject, string body)
    {
        try
        {
            MailSettings? settings = await _mailRepo.GetAsync();
            if (settings == null || !settings.IsActive)
                return (false, "Mail ayarlari bulunamadi veya pasif.");
            string? fullBody = body;
            if (!string.IsNullOrEmpty(settings.Signature))
                fullBody += "<br/><hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>" + settings.Signature;
            using SmtpClient client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                Credentials = new NetworkCredential(settings.SmtpUser, settings.SmtpPassword),
                EnableSsl = settings.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000
            };
            MailMessage mail = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromName),
                Subject = subject,
                Body = fullBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);
            await client.SendMailAsync(mail);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}