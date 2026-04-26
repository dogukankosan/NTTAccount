using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Business.Validators;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class AdminSiteSettingsController : AdminBaseController
{
    private readonly ISiteSettingsRepository _settingsRepository;
    private readonly ILogService _logService;
    public AdminSiteSettingsController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _settingsRepository = siteSettingsRepository;
        _logService = logService;
    }
    [HttpGet]
    public new async Task<IActionResult> Index()
    {
        SiteSettings? settings = await _settingsRepository.GetAsync();
        if (settings == null) return View(new SiteSettingsViewModel());
        return View(new SiteSettingsViewModel
        {
            Id = settings.Id,
            SiteName = settings.SiteName,
            SiteDescription = settings.SiteDescription,
            SiteUrl = settings.SiteUrl,
            SiteLogo = settings.SiteLogo,
            SiteIcon = settings.SiteIcon,
            Email = settings.Email,
            WhatsApp = settings.WhatsApp,
            Telegram = settings.Telegram,
            Facebook = settings.Facebook,
            Discord = settings.Discord,
            YouTube = settings.YouTube,
            CeoName = settings.CeoName,
            CeoTitle = settings.CeoTitle,
            CeoImage = settings.CeoImage,
            CeoDescription = settings.CeoDescription,
            AboutText = settings.AboutText
        });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSettingsViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (model.SiteLogoFile?.Length > 0) model.SiteLogo = await ToBase64(model.SiteLogoFile);
            if (model.SiteIconFile?.Length > 0) model.SiteIcon = await ToBase64(model.SiteIconFile);
            if (model.CeoImageFile?.Length > 0) model.CeoImage = await ToBase64(model.CeoImageFile);
            SiteSettingsValidator v = new SiteSettingsValidator().Validate(model.SiteName, model.SiteDescription, model.SiteUrl,
                model.SiteLogo, model.SiteIcon, model.Email, model.WhatsApp, model.Telegram,
                model.Facebook, model.Discord, model.YouTube, model.CeoName, model.CeoTitle, model.CeoDescription, model.AboutText);
            if (!v.IsValid) { foreach (var e in v.Errors) ModelState.AddModelError(string.Empty, e); return View(model); }
            SiteSettings s = new SiteSettings
            {
                Id = model.Id,
                SiteName = model.SiteName,
                SiteDescription = model.SiteDescription,
                SiteUrl = model.SiteUrl,
                SiteLogo = model.SiteLogo ?? "",
                SiteIcon = model.SiteIcon ?? "",
                Email = model.Email,
                WhatsApp = model.WhatsApp,
                Telegram = model.Telegram,
                Facebook = model.Facebook,
                Discord = model.Discord,
                YouTube = model.YouTube,
                CeoName = model.CeoName,
                CeoTitle = model.CeoTitle,
                CeoImage = model.CeoImage,
                CeoDescription = model.CeoDescription,
                AboutText = model.AboutText
            };
            bool result = await _settingsRepository.UpdateAsync(s, adminId);
            TempData[result ? "Success" : "Error"] = result ? "Ayarlar güncellendi." : "Güncelleme başarısız.";
        }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminSiteSettings.Index", adminId); TempData["Error"] = "Bir hata oluştu."; }
        return RedirectToAction("Index");
    }
    private static async Task<string> ToBase64(IFormFile file)
    {
        using MemoryStream ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
    }
}