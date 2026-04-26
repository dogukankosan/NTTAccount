using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class HomeController : UserBaseController
{
    private readonly IBannerSlideRepository _bannerRepo;
    private readonly ILogService _logService;

    public HomeController(
        ISiteSettingsRepository siteSettingsRepository,
        IBannerSlideRepository bannerRepo,
        ILogService logService)
        : base(siteSettingsRepository)
    {
        _bannerRepo = bannerRepo;
        _logService = logService;
    }
    public async Task<IActionResult> Index()
    {
        // Sadece aktif banner'larý çek
        var banners = await _bannerRepo.GetActiveAsync();
        ViewBag.Banners = banners;
        return View();
    }
}