using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Controllers;

public abstract class AdminBaseController : Controller
{
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUserRepository _userRepository;

    protected AdminBaseController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository)
    {
        _siteSettingsRepository = siteSettingsRepository;
        _contactRepository = contactRepository;
        _userRepository = userRepository;
    }
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        SiteSettings settings = await _siteSettingsRepository.GetAsync();
        if (settings != null)
        {
            ViewBag.SiteName = settings.SiteName;
            ViewBag.SiteDescription = settings.SiteDescription;
            ViewBag.SiteUrl = settings.SiteUrl;
            ViewBag.SiteLogo = settings.SiteLogo;
            ViewBag.SiteIcon = settings.SiteIcon;
            ViewBag.Email = settings.Email;
            ViewBag.WhatsApp = settings.WhatsApp;
            ViewBag.Telegram = settings.Telegram;
            ViewBag.Facebook = settings.Facebook;
            ViewBag.Discord = settings.Discord;
            ViewBag.YouTube = settings.YouTube;
        }
        // Okunmamış mesaj sayısı
        var contacts = await _contactRepository.GetAllAsync();
        ViewBag.UnreadContactCount = contacts.Count(x => !x.IsRead);
        // Admin bilgileri
        ViewBag.AdminEmail = context.HttpContext.Items["AdminEmail"]?.ToString();
        ViewBag.AdminUserId = context.HttpContext.Items["AdminUserId"];
        ViewBag.AdminRoleId = context.HttpContext.Items["AdminRoleId"] is byte r ? (int)r : 2;
        // Admin profil resmi
        if (context.HttpContext.Items["AdminUserId"] is int adminId)
        {
            var adminUser = await _userRepository.GetByIdAsync(adminId);
            ViewBag.AdminProfileImage = adminUser?.ProfileImage;
        }
        await next();
    }
}