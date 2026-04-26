using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Business.Validators;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Security;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class AdminUserController : AdminBaseController
{
    private readonly IUserRepository _userRepository;
    private readonly ILogService _logService;
    private const string ProtectedEmail = "admin@dogukankosan.com";

    public AdminUserController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _userRepository = userRepository;
        _logService = logService;
    }

    [HttpGet]
    public new async Task<IActionResult> Index() => View(await _userRepository.GetAllAsync());

    [HttpGet]
    public IActionResult Create() => View(new AdminUserViewModel { IsActive = true, RoleId = 2 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (model.ProfileImageFile?.Length > 0) model.ProfileImage = await ToBase64(model.ProfileImageFile);
            AdminUserValidator v = new AdminUserValidator().Validate(model.Email, model.Password, model.RoleId, isNew: true);
            if (!v.IsValid) { foreach (var e in v.Errors) ModelState.AddModelError(string.Empty, e); return View(model); }
            if (await _userRepository.EmailExistsAsync(model.Email)) { ModelState.AddModelError(string.Empty, "Bu email zaten kayıtlı."); return View(model); }
            string? hash = PasswordHasher.Hash(model.Password!);
            await _userRepository.CreateAsync(new User { Email = model.Email.Trim().ToLowerInvariant(), RoleId = model.RoleId, IsActive = model.IsActive, Note = model.Note, ProfileImage = model.ProfileImage }, hash);
            TempData["Success"] = "Kullanıcı eklendi.";
            return RedirectToAction("Index");
        }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminUser.Create", adminId); ModelState.AddModelError(string.Empty, "Hata oluştu."); }
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return RedirectToAction("Index");
        if (user.Email.ToLower() == ProtectedEmail) { TempData["Error"] = "Bu kullanıcıya müdahale edilemez."; return RedirectToAction("Index"); }
        return View(new AdminUserViewModel { Id = user.Id, Email = user.Email, RoleId = user.RoleId, IsActive = user.IsActive, Note = user.Note, ProfileImage = user.ProfileImage });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminUserViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        var existing = await _userRepository.GetByIdAsync(model.Id);
        if (existing?.Email.ToLower() == ProtectedEmail) { TempData["Error"] = "Bu kullanıcıya müdahale edilemez."; return RedirectToAction("Index"); }
        try
        {
            if (model.ProfileImageFile?.Length > 0) model.ProfileImage = await ToBase64(model.ProfileImageFile);
            AdminUserValidator v = new AdminUserValidator().Validate(model.Email, model.Password, model.RoleId, isNew: false);
            if (!v.IsValid) { foreach (var e in v.Errors) ModelState.AddModelError(string.Empty, e); return View(model); }
            if (await _userRepository.EmailExistsAsync(model.Email, model.Id)) { ModelState.AddModelError(string.Empty, "Bu email başka kullanıcıya ait."); return View(model); }
            await _userRepository.AdminUpdateAsync(new User { Id = model.Id, Email = model.Email.Trim().ToLowerInvariant(), RoleId = model.RoleId, IsActive = model.IsActive, Note = model.Note, ProfileImage = model.ProfileImage });
            if (!string.IsNullOrEmpty(model.Password)) await _userRepository.AdminUpdatePasswordAsync(model.Id, PasswordHasher.Hash(model.Password));
            TempData["Success"] = "Kullanıcı güncellendi.";
            return RedirectToAction("Index");
        }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminUser.Edit", adminId); ModelState.AddModelError(string.Empty, "Hata oluştu."); }
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (id == adminId) { TempData["Error"] = "Kendi hesabınızı silemezsiniz."; return RedirectToAction("Index"); }
        var user = await _userRepository.GetByIdAsync(id);
        if (user?.Email.ToLower() == ProtectedEmail) { TempData["Error"] = "Bu kullanıcı silinemez."; return RedirectToAction("Index"); }
        try { await _userRepository.DeleteAsync(id); TempData["Success"] = "Kullanıcı silindi."; }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminUser.Delete", adminId); TempData["Error"] = "Silme başarısız."; }
        return RedirectToAction("Index");
    }
    private static async Task<string> ToBase64(IFormFile file)
    {
        using MemoryStream ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
    }
}