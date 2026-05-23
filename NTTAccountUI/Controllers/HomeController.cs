using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class HomeController : UserBaseController
{
    private readonly IBannerSlideRepository _bannerRepo;
    private readonly IProductRepository _productRepo; // ✅ ekle

    public HomeController(
        ISiteSettingsRepository siteSettingsRepository,
        IBannerSlideRepository bannerRepo,
        IProductRepository productRepo) // ✅ ekle
        : base(siteSettingsRepository)
    {
        _bannerRepo = bannerRepo;
        _productRepo = productRepo; // ✅ ekle
    }
    public async Task<IActionResult> Index()
    {
     
        ViewBag.Banners = await _bannerRepo.GetActiveAsync();
        ViewBag.Products = await _productRepo.GetActiveAsync(); // ✅ ekle
        return View();
    }
}