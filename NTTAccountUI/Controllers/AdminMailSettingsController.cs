using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class AdminMailSettingsController : AdminBaseController
{
    private readonly IMailSettingsRepository _mailRepo;
    private readonly IMailService _mailService;
    private readonly ILogService _logService;
    private const string TestPassedKey = "MailTestPassed";
    public AdminMailSettingsController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IMailSettingsRepository mailRepo,
        IMailService mailService,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _mailRepo = mailRepo;
        _mailService = mailService;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        MailSettings? settings = await _mailRepo.GetAsync();
        MailSettingsViewModel model = settings != null
            ? new MailSettingsViewModel
            {
                Id = settings.Id,
                SmtpHost = settings.SmtpHost,
                SmtpPort = settings.SmtpPort,
                SmtpUser = settings.SmtpUser,
                FromName = settings.FromName,
                FromEmail = settings.FromEmail,
                UseSsl = settings.UseSsl,
                Signature = settings.Signature,
                IsActive = settings.IsActive
            }
            : new MailSettingsViewModel { SmtpPort = 587, UseSsl = true };
        model.TestPassed = HttpContext.Session.GetString(TestPassedKey) == "1";
        return View(model);
    }
    // JSON body ile test — [FromBody] şart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTest([FromBody] MailSettingsViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (string.IsNullOrWhiteSpace(model.TestEmail))
            return Json(new { success = false, error = "Test email adresi giriniz." });
        if (string.IsNullOrWhiteSpace(model.SmtpHost))
            return Json(new { success = false, error = "SMTP sunucu adresi zorunludur." });
        // Sifre bossa DB'den al
        string? smtpPassword = model.SmtpPassword;
        if (string.IsNullOrEmpty(smtpPassword))
        {
            MailSettings? existing = await _mailRepo.GetAsync();
            if (existing == null || string.IsNullOrEmpty(existing.SmtpPassword))
                return Json(new { success = false, error = "Sifre giriniz." });
            smtpPassword = existing.SmtpPassword;
        }
        var (success, error) = await _mailService.SendTestAsync(
            model.SmtpHost,
            model.SmtpPort > 0 ? model.SmtpPort : 587,
            model.SmtpUser,
            smtpPassword,
            model.FromName,
            model.FromEmail,
            model.UseSsl,
            model.TestEmail!,
            model.Signature
        );
        if (success)
        {
            HttpContext.Session.SetString(TestPassedKey, "1");
            await _logService.LogInfoAsync($"Mail test basarili: {model.TestEmail}", "AdminMailSettings", adminId);
        }
        else
            HttpContext.Session.Remove(TestPassedKey);
        return Json(new { success, error });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(MailSettingsViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (HttpContext.Session.GetString(TestPassedKey) != "1")
        {
            TempData["Error"] = "Kaydetmeden once test maili basariyla gonderilmelidir.";
            model.TestPassed = false;
            return View("Index", model);
        }
        try
        {
            MailSettings settings = new MailSettings
            {
                Id = model.Id,
                SmtpHost = model.SmtpHost,
                SmtpPort = model.SmtpPort,
                SmtpUser = model.SmtpUser,
                SmtpPassword = model.SmtpPassword ?? string.Empty,
                FromName = model.FromName,
                FromEmail = model.FromEmail,
                UseSsl = model.UseSsl,
                Signature = model.Signature,
                IsActive = model.IsActive
            };
            await _mailRepo.UpdateAsync(settings, adminId);
            HttpContext.Session.Remove(TestPassedKey);
            TempData["Success"] = "Mail ayarlari basariyla kaydedildi.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminMailSettings.Save", adminId);
            TempData["Error"] = "Bir hata olustu.";
        }
        return RedirectToAction("Index");
    }
}