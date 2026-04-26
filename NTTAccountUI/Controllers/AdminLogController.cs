using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class AdminLogController : AdminBaseController
{
    private readonly IErrorLogRepository _logRepository;
    private readonly ILogService _logService;

    public AdminLogController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IErrorLogRepository logRepository,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _logRepository = logRepository;
        _logService = logService;
    }

    [HttpGet]
    public new async Task<IActionResult> Index()
    {
        var logs = await _logRepository.GetAllAsync();
        return View(logs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try { await _logRepository.DeleteAsync(id); TempData["Success"] = "Log silindi."; }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminLog.Delete", adminId); TempData["Error"] = "Silme başarısız."; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll()
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try { await _logRepository.DeleteAllAsync(); TempData["Success"] = "Tüm loglar silindi."; }
        catch (Exception ex) { await _logService.LogErrorAsync(ex, "AdminLog.DeleteAll", adminId); TempData["Error"] = "İşlem başarısız."; }
        return RedirectToAction("Index");
    }
}