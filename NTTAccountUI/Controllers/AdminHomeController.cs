using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;

namespace NTTAccountUI.Controllers;

public class AdminHomeController : AdminBaseController
{
    private readonly IUserRepository _userRepo;
    private readonly IContactRepository _contactRepo;
    private readonly IErrorLogRepository _logRepo;
    private readonly IBannerSlideRepository _bannerRepo;

    public AdminHomeController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IErrorLogRepository logRepository,
        IBannerSlideRepository bannerRepository)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _userRepo = userRepository;
        _contactRepo = contactRepository;
        _logRepo = logRepository;
        _bannerRepo = bannerRepository;
    }
    public async Task<IActionResult> Index()
    {
        int roleId = HttpContext.Items["AdminRoleId"] is byte r ? (int)r : 2;
        if (roleId == 1)
        {
            // Admin dashboard — istatistikler
            var users = await _userRepo.GetAllAsync();
            var contacts = await _contactRepo.GetAllAsync();
            var logs = await _logRepo.GetAllAsync();
            var banners = await _bannerRepo.GetAllAsync();
            ViewBag.TotalUsers = users.Count();
            ViewBag.ActiveUsers = users.Count(x => x.IsActive);
            ViewBag.TotalContacts = contacts.Count();
            ViewBag.UnreadContacts = contacts.Count(x => !x.IsRead);
            ViewBag.TotalLogs = logs.Count();
            ViewBag.ErrorLogs = logs.Count(x => x.Level == "Error" || x.Level == "Critical");
            ViewBag.TotalBanners = banners.Count();
            ViewBag.ActiveBanners = banners.Count(x => x.IsActive);
            // Son 5 mesaj
            ViewBag.RecentContacts = contacts.Take(5).ToList();
            // Son 5 log
            ViewBag.RecentLogs = logs.Take(5).ToList();
            // Son 5 kullanıcı
            ViewBag.RecentUsers = users.Take(5).ToList();
            return View("AdminDashboard");
        }
        else
        {
            // User dashboard
            int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
            var user = await _userRepo.GetByIdAsync(userId);
            ViewBag.CurrentUser = user;
            return View("UserDashboard");
        }
    }
}