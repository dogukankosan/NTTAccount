using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Controllers;

public abstract class UserBaseController : Controller
{
    private readonly ISiteSettingsRepository _siteSettingsRepository;

    protected UserBaseController(ISiteSettingsRepository siteSettingsRepository)
    {
        _siteSettingsRepository = siteSettingsRepository;
    }
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        SiteSettings? settings = await _siteSettingsRepository.GetAsync();
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
            ViewBag.CeoName = settings.CeoName;
            ViewBag.CeoTitle = settings.CeoTitle;
            ViewBag.CeoImage = settings.CeoImage;
            ViewBag.CeoDescription = settings.CeoDescription;
            ViewBag.AboutText = settings.AboutText;
        }
        await next();
    }
}