using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class AdminContactController : AdminBaseController
{
    private readonly IContactRepository _contactRepository;
    private readonly ILogService _logService;

    public AdminContactController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _contactRepository = contactRepository;
        _logService = logService;
    }
    [HttpGet]
    public new async Task<IActionResult> Index()
    {
        var contacts = await _contactRepository.GetAllAsync();
        return View(contacts);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRead(int id)
    {
        try { await _contactRepository.ToggleReadAsync(id); }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminContact.ToggleRead"); }
        return RedirectToAction("Index");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try { await _contactRepository.DeleteAsync(id); TempData["Success"] = "Mesaj silindi."; }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminContact.Delete", adminId); TempData["Error"] = "Silme başarısız."; }
        return RedirectToAction("Index");
    }
}