using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;

namespace NTTAccountUI.Controllers;

public class AdminHomeController : AdminBaseController
{
    private readonly IUserRepository _userRepo;
    private readonly IContactRepository _contactRepo;
    private readonly IErrorLogRepository _logRepo;
    private readonly IBannerSlideRepository _bannerRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public AdminHomeController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IErrorLogRepository logRepository,
        IBannerSlideRepository bannerRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _userRepo = userRepository;
        _contactRepo = contactRepository;
        _logRepo = logRepository;
        _bannerRepo = bannerRepository;
        _orderRepo = orderRepository;
        _productRepo = productRepository;
    }
    public async Task<IActionResult> Index()
    {
        int roleId = HttpContext.Items["AdminRoleId"] is byte r ? (int)r : 2;

        if (roleId == 1)
        {
            var users = await _userRepo.GetAllAsync();
            var contacts = await _contactRepo.GetAllAsync();
            var logs = await _logRepo.GetAllAsync();
            var banners = await _bannerRepo.GetAllAsync();
            var orders = await _orderRepo.GetAllAsync();
            var products = await _productRepo.GetAllAsync();
            // Kullanıcılar
            ViewBag.TotalUsers = users.Count();
            ViewBag.ActiveUsers = users.Count(x => x.IsActive);
            // Mesajlar
            ViewBag.TotalContacts = contacts.Count();
            ViewBag.UnreadContacts = contacts.Count(x => !x.IsRead);
            // Loglar
            ViewBag.TotalLogs = logs.Count();
            ViewBag.ErrorLogs = logs.Count(x => x.Level == "Error" || x.Level == "Critical");
            // Bannerlar
            ViewBag.TotalBanners = banners.Count();
            ViewBag.ActiveBanners = banners.Count(x => x.IsActive);
            // Siparişler
            ViewBag.TotalOrders = orders.Count();
            ViewBag.PendingOrders = orders.Count(x => x.Status == 0);
            ViewBag.PartialOrders = orders.Count(x => x.Status == 1);
            ViewBag.ClosedOrders = orders.Count(x => x.Status == 2);
            ViewBag.TotalRevenue = orders
                .SelectMany(o => o.Items)
                .Sum(i => i.UnitPrice * i.Quantity);
            ViewBag.RecentOrders = orders.Take(5).ToList();
            // Ürünler
            ViewBag.TotalProducts = products.Count();
            ViewBag.ActiveProducts = products.Count(x => x.IsActive);
            ViewBag.LowStockProducts = products
                .Where(x => x.IsActive && x.Stock <= 5)
                .OrderBy(x => x.Stock)
                .Take(5)
                .ToList();
            // Son veriler
            ViewBag.RecentContacts = contacts.Take(5).ToList();
            ViewBag.RecentLogs = logs.Take(5).ToList();
            ViewBag.RecentUsers = users.Take(5).ToList();
            return View("AdminDashboard");
        }
        else
        {
            int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
            var user = await _userRepo.GetByIdAsync(userId);
            // User için sipariş istatistikleri
            var userOrders = await _orderRepo.GetByUserIdAsync(userId);
            ViewBag.CurrentUser = user;
            ViewBag.UserTotalOrders = userOrders.Count();
            ViewBag.UserPendingOrders = userOrders.Count(x => x.Status == 0);
            ViewBag.UserClosedOrders = userOrders.Count(x => x.Status == 2);
            ViewBag.UserRecentOrders = userOrders.Take(3).ToList();
            return View("UserDashboard");
        }
    }
}