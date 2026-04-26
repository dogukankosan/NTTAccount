using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Business.Validators;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace NTTAccountUI.Controllers;

public class AdminBannerSlideController : AdminBaseController
{
    private readonly IBannerSlideRepository _bannerRepo;
    private readonly ILogService _logService;
    // Maksimum boyutlar ve kalite
    private const int MaxWidth = 1920;
    private const int MaxHeight = 1080;
    private const int JpegQuality = 75;
    public AdminBannerSlideController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IBannerSlideRepository bannerRepo,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _bannerRepo = bannerRepo;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await _bannerRepo.GetAllAsync());

    [HttpGet]
    public IActionResult Create() => View(new BannerSlideViewModel { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BannerSlideViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (model.ImageFile?.Length > 0)
                model.Image = await CompressImageAsync(model.ImageFile);
            BannerSlideValidator v = new BannerSlideValidator().Validate(model.Title, model.Description, model.Image, isNew: true);
            if (!v.IsValid)
            {
                foreach (var e in v.Errors) ModelState.AddModelError(string.Empty, e);
                return View(model);
            }
            await _bannerRepo.CreateAsync(new BannerSlide
            {
                Title = model.Title,
                Description = model.Description,
                Image = model.Image!,
                OrderNo = model.OrderNo,
                IsActive = model.IsActive
            });
            TempData["Success"] = "Banner eklendi.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminBannerSlide.Create", adminId);
            ModelState.AddModelError(string.Empty, "Hata oluştu.");
        }
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        BannerSlide? s = await _bannerRepo.GetByIdAsync(id);
        if (s == null) return RedirectToAction("Index");
        return View(new BannerSlideViewModel
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            Image = s.Image,
            OrderNo = s.OrderNo,
            IsActive = s.IsActive
        });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BannerSlideViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (model.ImageFile?.Length > 0)
                model.Image = await CompressImageAsync(model.ImageFile);
            BannerSlideValidator v = new BannerSlideValidator().Validate(model.Title, model.Description, model.Image, isNew: false);
            if (!v.IsValid)
            {
                foreach (var e in v.Errors) ModelState.AddModelError(string.Empty, e);
                return View(model);
            }
            await _bannerRepo.UpdateAsync(new BannerSlide
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Image = model.Image ?? string.Empty,
                OrderNo = model.OrderNo,
                IsActive = model.IsActive
            });
            TempData["Success"] = "Banner güncellendi.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminBannerSlide.Edit", adminId);
            ModelState.AddModelError(string.Empty, "Hata oluştu.");
        }
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            await _bannerRepo.DeleteAsync(id);
            TempData["Success"] = "Banner silindi.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminBannerSlide.Delete", adminId);
            TempData["Error"] = "Silme başarısız.";
        }
        return RedirectToAction("Index");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _bannerRepo.ToggleActiveAsync(id);
        return RedirectToAction("Index");
    }
    // Görseli sıkıştır → max 1920x1080, JPEG %75 kalite → Base64
    private static async Task<string> CompressImageAsync(IFormFile file)
    {
        using Stream inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream);
        // Boyut küçültme — oranı koru
        if (image.Width > MaxWidth || image.Height > MaxHeight)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(MaxWidth, MaxHeight),
                Mode = ResizeMode.Max
            }));
        }
        // JPEG olarak sıkıştır
        using MemoryStream outputStream = new MemoryStream();
        JpegEncoder encoder = new JpegEncoder { Quality = JpegQuality };
        await image.SaveAsync(outputStream, encoder);
        string? base64 = Convert.ToBase64String(outputStream.ToArray());
        return $"data:image/jpeg;base64,{base64}";
    }
}