using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class AdminProductController : AdminBaseController
{
    private readonly IProductRepository _productRepository;
    private readonly ILogService _logService;
    public AdminProductController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _productRepository = productRepository;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await _productRepository.GetAllAsync());

    [HttpGet]
    public IActionResult Create()
        => View(new ProductViewModel { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (!ModelState.IsValid) return View(model);
        try
        {
            if (await _productRepository.CodeExistsAsync(model.Code))
            {
                ModelState.AddModelError(string.Empty, "Bu ürün kodu zaten kayıtlı.");
                return View(model);
            }
            await _productRepository.CreateAsync(new Product
            {
                Code = model.Code.Trim().ToUpperInvariant(),
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                Stock = model.Stock,
                IsActive = model.IsActive,
                CreatedBy = adminId
            });
            TempData["Success"] = "Ürün eklendi.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminProduct.Create", adminId);
            ModelState.AddModelError(string.Empty, "Hata oluştu.");
        }
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        Product? product = await _productRepository.GetByIdAsync(id);
        if (product == null) return RedirectToAction("Index");
        return View(new ProductViewModel
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (!ModelState.IsValid) return View(model);
        try
        {
            if (await _productRepository.CodeExistsAsync(model.Code, model.Id))
            {
                ModelState.AddModelError(string.Empty, "Bu ürün kodu başka ürüne ait.");
                return View(model);
            }
            await _productRepository.UpdateAsync(new Product
            {
                Id = model.Id,
                Code = model.Code.Trim().ToUpperInvariant(),
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                Stock = model.Stock,
                IsActive = model.IsActive
            });
            TempData["Success"] = "Ürün güncellendi.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminProduct.Edit", adminId);
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
            if (await _productRepository.HasOrderAsync(id))
            {
                TempData["Error"] = "Bu ürün bir siparişe bağlı, silinemez.";
                return RedirectToAction("Index");
            }
            await _productRepository.DeleteAsync(id);
            TempData["Success"] = "Ürün silindi.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminProduct.Delete", adminId);
            TempData["Error"] = "Silme başarısız.";
        }
        return RedirectToAction("Index");
    }
}