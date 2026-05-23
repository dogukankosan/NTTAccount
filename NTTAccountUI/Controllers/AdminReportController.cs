using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Security;

namespace NTTAccountUI.Controllers;
public class AdminReportController : AdminBaseController
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository2;
    private readonly IEncryptionService _encryptionService;
    public AdminReportController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEncryptionService encryptionService)   // ✅
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository2 = userRepository;
        _encryptionService = encryptionService; // ✅
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _orderRepository.GetAllAsync();
        var products = await _productRepository.GetAllAsync();
        var users = await _userRepository2.GetAllAsync();
        var orderData = orders.Select(o => new
        {
            id = o.Id,
            orderNo = o.OrderNo,
            userEmail = o.UserEmail,
            status = o.Status,
            createdAt = o.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd"),
            createdAtDisplay = o.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
            note = o.Note ?? "",
            items = o.Items.Select(i => new
            {
                id = i.Id,
                productName = i.ProductName,
                productCode = i.ProductCode,
                serverName = i.ServerName,
                characterId = i.CharacterId,
                characterPw = _encryptionService.Decrypt(i.CharacterPw),      // ✅
                characterMail = i.CharacterMail,
                characterMailPw = _encryptionService.Decrypt(i.CharacterMailPw),  // ✅
                otpCode = i.OtpCode,
                otpPassword = _encryptionService.Decrypt(i.OtpPassword),      // ✅
                quantity = i.Quantity,
                unitPrice = i.UnitPrice,
                total = i.UnitPrice * i.Quantity,
                isClosed = i.IsClosed
            }).ToList()
        }).ToList();
        ViewBag.OrderDataJson = System.Text.Json.JsonSerializer.Serialize(orderData);
        ViewBag.TotalOrders = orders.Count();
        ViewBag.TotalRevenue = orders.SelectMany(o => o.Items).Sum(i => i.UnitPrice * i.Quantity);
        ViewBag.TotalProducts = products.Count();
        ViewBag.TotalUsers = users.Count(u => u.RoleId == 2);
        return View();
    }
}