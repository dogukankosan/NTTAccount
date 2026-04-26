using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Business.Validators;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Security;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class AdminLoginController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IAdminSessionRepository _sessionRepository;
    private readonly ILogService _logService;
    private readonly ISiteSettingsRepository _siteSettingsRepository;

    public AdminLoginController(
        IUserRepository userRepository,
        IAdminSessionRepository sessionRepository,
        ILogService logService,
        ISiteSettingsRepository siteSettingsRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _logService = logService;
        _siteSettingsRepository = siteSettingsRepository;
    }
    private async Task LoadSiteSettings()
    {
        SiteSettings? settings = await _siteSettingsRepository.GetAsync();
        if (settings == null) return;
        ViewBag.SiteName = settings.SiteName;
        ViewBag.SiteLogo = settings.SiteLogo;
        ViewBag.SiteIcon = settings.SiteIcon;
        ViewBag.Email = settings.Email;
        ViewBag.WhatsApp = settings.WhatsApp;
        ViewBag.Telegram = settings.Telegram;
        ViewBag.Discord = settings.Discord;
        ViewBag.Facebook = settings.Facebook;
        ViewBag.YouTube = settings.YouTube;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (Request.Cookies.ContainsKey("AdminSession"))
            return RedirectToAction("Index", "AdminHome");
        await LoadSiteSettings();
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AdminLoginViewModel model)
    {
        await LoadSiteSettings();
        if (!ModelState.IsValid)
            return View(model);
        AdminLoginValidator validator = new AdminLoginValidator().Validate(model.Email, model.Password);
        if (!validator.IsValid)
        {
            foreach (string error in validator.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        try
        {
            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user == null)
            {
                await Task.Delay(500);
                ModelState.AddModelError("", "Email veya şifre hatalı.");
                return View(model);
            }
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Hesabınız pasif.");
                return View(model);
            }
            if (user.LockoutUntil.HasValue && user.LockoutUntil > DateTime.UtcNow)
            {
                int remaining = (user.LockoutUntil.Value - DateTime.UtcNow).Minutes + 1;
                ModelState.AddModelError("", $"Kilitli. {remaining} dk sonra deneyin.");
                return View(model);
            }
            if (!PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                await _userRepository.UpdateLoginInfoAsync(user.Id, ipAddress, false);
                ModelState.AddModelError("", "Email veya şifre hatalı.");
                return View(model);
            }
            await _userRepository.UpdateLoginInfoAsync(user.Id, ipAddress, true);
            int expireHours = model.RememberMe ? 24 * 30 : 8;
            string? token = await _sessionRepository.CreateSessionAsync(
                user.Id,
                ipAddress,
                Request.Headers["User-Agent"].ToString(),
                expireHours
            );
            Response.Cookies.Append("AdminSession", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(expireHours)
            });
            await _logService.LogInfoAsync($"Giriş: {user.Email}", "AdminLogin", user.Id);
            return RedirectToAction("Index", "AdminHome");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminLoginController.Index");
            ModelState.AddModelError("", "Bir hata oluştu.");
            return View(model);
        }
    }
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        string? token = Request.Cookies["AdminSession"];
        if (!string.IsNullOrEmpty(token))
        {
            await _sessionRepository.RevokeSessionAsync(token);
            Response.Cookies.Delete("AdminSession");
        }
        return RedirectToAction("Index");
    }
}