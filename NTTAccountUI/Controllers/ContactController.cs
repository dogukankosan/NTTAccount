using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Business.Validators;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Security;
using NTTAccountUI.Services;
using System.Text.RegularExpressions;

namespace NTTAccountUI.Controllers;

public class ContactController : UserBaseController
{
    private readonly IContactRepository _contactRepository;
    private readonly ILogService _logService;
    private readonly IMailService _mailService;
    private readonly ISiteSettingsRepository _siteSettingsRepository;

    public ContactController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        ILogService logService,
        IMailService mailService)
        : base(siteSettingsRepository)
    {
        _siteSettingsRepository = siteSettingsRepository;
        _contactRepository = contactRepository;
        _logService = logService;
        _mailService = mailService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactViewModel model)
    {
        // Honeypot
        if (!string.IsNullOrEmpty(model.Website))
        {
            TempData["Success"] = "Mesajınız alındı, en kısa sürede dönüş yapacağız.";
            return RedirectToAction("Index");
        }
        if (!ModelState.IsValid)
            return View(model);
        ContactValidator validator = new ContactValidator()
            .Validate(model.FullName, model.Phone, model.Subject, model.Message);
        if (!validator.IsValid)
        {
            foreach (string error in validator.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        try
        {
            bool isSpam = await _contactRepository.HasSpamAsync(ipAddress, model.Phone);
            if (isSpam)
            {
                ModelState.AddModelError(string.Empty, "Çok fazla mesaj gönderdiniz. Lütfen daha sonra tekrar deneyin.");
                return View(model);
            }
            Contact contact = new Contact
            {
                FullName = InputSanitizer.Sanitize(model.FullName),
                Phone = InputSanitizer.Sanitize(model.Phone),
                Subject = InputSanitizer.Sanitize(model.Subject),
                Message = InputSanitizer.Sanitize(model.Message),
                IpAddress = ipAddress
            };
            bool result = await _contactRepository.CreateAsync(contact);
            if (result)
            {
                // DB'ye kaydedildikten sonra kendine bildirim maili gönder
                // fire-and-forget — kullanıcıyı bekletme
                _ = SendNotificationMailAsync(contact);
                TempData["Success"] = "Mesajınız alındı, en kısa sürede dönüş yapacağız.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "Mesaj gönderilemedi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "ContactController.Index");
            ModelState.AddModelError(string.Empty, "Bir hata oluştu. Lütfen tekrar deneyin.");
        }
        return View(model);
    }
    // ─── Bildirim maili ───────────────────────────────────────────────────────
    private async Task SendNotificationMailAsync(Contact contact)
    {
        try
        {
            SiteSettings? settings = await _siteSettingsRepository.GetAsync();
            string? adminEmail = settings?.Email; // SiteSettings'teki Email alanı
            if (string.IsNullOrWhiteSpace(adminEmail))
                return;
            string siteName = settings?.SiteName ?? "NTT Hesap";
            string subject = $"📩 Yeni İletişim Mesajı — {contact.FullName}";
            // Telefonu WA linkine uygun formata getir
            string waPhone = contact.Phone?.Replace("+", "").Replace(" ", "") ?? "";
            // Bunu ekle
            string phoneDigits = Regex.Replace(contact.Phone ?? "", @"\D", "");
            if (phoneDigits.StartsWith("90")) phoneDigits = phoneDigits.Substring(2);
            if (phoneDigits.StartsWith("0")) phoneDigits = phoneDigits.Substring(1);
            if (phoneDigits.Length > 10) phoneDigits = phoneDigits.Substring(0, 10);
            string displayPhone = phoneDigits.Length == 10
                ? $"+90 {phoneDigits.Substring(0, 3)} {phoneDigits.Substring(3, 3)} {phoneDigits.Substring(6, 2)} {phoneDigits.Substring(8, 2)}"
                : contact.Phone ?? "";
            string body = $@"<!DOCTYPE html>
<html lang='tr'>
<head><meta charset='UTF-8'></head>
<body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f4f4;padding:30px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0'
               style='background:#ffffff;border-radius:8px;overflow:hidden;
                      box-shadow:0 2px 8px rgba(0,0,0,0.08);'>

          <!-- Header -->
          <tr>
            <td style='background:#e84545;padding:28px 32px;'>
              <h1 style='margin:0;color:#fff;font-size:20px;letter-spacing:1px;'>
                📩 Yeni İletişim Mesajı
              </h1>
              <p style='margin:6px 0 0;color:rgba(255,255,255,0.8);font-size:13px;'>
             {DateTime.Now:dd.MM.yyyy HH:mm} &nbsp;·&nbsp; IP: {contact.IpAddress}

              </p>
            </td>
          </tr>

          <!-- İçerik -->
          <tr>
            <td style='padding:28px 32px;'>
              <table width='100%' cellpadding='0' cellspacing='0'>

                <tr>
                  <td style='padding:10px 0;border-bottom:1px solid #f0f0f0;'>
                    <span style='color:#888;font-size:12px;text-transform:uppercase;letter-spacing:1px;'>Ad Soyad</span><br>
                    <span style='color:#222;font-size:15px;font-weight:bold;'>{contact.FullName}</span>
                  </td>
                </tr>

                <tr>
                  <td style='padding:10px 0;border-bottom:1px solid #f0f0f0;'>
                    <span style='color:#888;font-size:12px;text-transform:uppercase;letter-spacing:1px;'>Telefon</span><br>
                    <a href='https://wa.me/{waPhone}'
                       style='color:#25d366;font-size:15px;text-decoration:none;font-weight:bold;'>
                 📱 {displayPhone}
                    </a>
                  </td>
                </tr>

                <tr>
                  <td style='padding:10px 0;border-bottom:1px solid #f0f0f0;'>
                    <span style='color:#888;font-size:12px;text-transform:uppercase;letter-spacing:1px;'>Konu</span><br>
                    <span style='color:#222;font-size:15px;'>{contact.Subject}</span>
                  </td>
                </tr>

                <tr>
                  <td style='padding:10px 0;'>
                    <span style='color:#888;font-size:12px;text-transform:uppercase;letter-spacing:1px;'>Mesaj</span><br>
                    <p style='color:#333;font-size:14px;line-height:1.7;margin:8px 0 0;
                              background:#f9f9f9;border-left:3px solid #e84545;
                              padding:12px 16px;border-radius:4px;'>
                      {contact.Message?.Replace("\n", "<br/>")}
                    </p>
                  </td>
                </tr>

              </table>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='background:#f9f9f9;padding:18px 32px;border-top:1px solid #eee;'>
              <p style='margin:0;color:#aaa;font-size:12px;'>
                Bu mail <strong>{siteName}</strong> sitesinden otomatik gönderilmiştir.
                Admin panelinden tüm mesajları görüntüleyebilirsiniz.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
            await _mailService.SendAsync(adminEmail, subject, body);
        }
        catch
        {
            // Mail gönderilemese bile kullanıcıya hata gösterme, sessizce geç
        }
    }
}