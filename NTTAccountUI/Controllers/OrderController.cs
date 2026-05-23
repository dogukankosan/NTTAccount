using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Security;
using NTTAccountUI.Services;
using System.IO.Compression;

namespace NTTAccountUI.Controllers;
public class OrderController : AdminBaseController
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogService _logService;

    public OrderController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        IEncryptionService encryptionService,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _orderRepository = orderRepository;
        _encryptionService = encryptionService;
        _logService = logService;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (userId == 0) return RedirectToAction("Index", "AdminLogin");
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        foreach (Order? order in orders)
            foreach (OrderItem? item in order.Items)
            {
                item.CharacterPw = _encryptionService.Decrypt(item.CharacterPw);
                item.CharacterMailPw = _encryptionService.Decrypt(item.CharacterMailPw);
                item.OtpPassword = _encryptionService.Decrypt(item.OtpPassword);
            }
        return View(orders);
    }
    [HttpGet]
    public async Task<IActionResult> DownloadZip(int itemId, int orderId)
    {
        int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (userId == 0) return RedirectToAction("Index", "AdminLogin");
        Order? order = await _orderRepository.GetByIdForUserAsync(orderId, userId);
        if (order == null) return RedirectToAction("Index");
        OrderItem? item = order.Items.FirstOrDefault(x => x.Id == itemId && x.IsClosed);
        if (item == null || string.IsNullOrEmpty(item.ZipFile))
            return RedirectToAction("Index");
        string? charPw = _encryptionService.Decrypt(item.CharacterPw);
        string? mailPw = _encryptionService.Decrypt(item.CharacterMailPw);
        string? otpPw = _encryptionService.Decrypt(item.OtpPassword);
        string? info = $"Sipariş No    : {order.OrderNo}\r\n" +
                   $"Ürün          : {item.ProductName}\r\n" +
                   $"Server        : {item.ServerName}\r\n" +
                   $"Karakter ID   : {item.CharacterId}\r\n" +
                   $"Karakter Şifre: {charPw}\r\n" +
                   $"Karakter Mail : {item.CharacterMail}\r\n" +
                   $"Mail Şifre    : {mailPw}\r\n" +
                   $"OTP Kodu      : {item.OtpCode}\r\n" +
                   $"OTP Şifre     : {otpPw}\r\n";
        string? fileName = $"{item.CharacterId}-{item.Id}";
        using MemoryStream memoryStream = new MemoryStream();
        using (ZipArchive archive = new ZipArchive(
            memoryStream, ZipArchiveMode.Create, true))
        {
            string? base64 = item.ZipFile.Contains(",")
                ? item.ZipFile.Split(',')[1]
                : item.ZipFile;
            byte[] zipBytes = Convert.FromBase64String(base64);
            ZipArchiveEntry zipEntry = archive.CreateEntry(
                $"{fileName}.zip", CompressionLevel.Fastest);
            using (Stream zipStream = zipEntry.Open())
                await zipStream.WriteAsync(zipBytes);
            ZipArchiveEntry txtEntry = archive.CreateEntry(
                $"{fileName}_bilgiler.txt", CompressionLevel.Fastest);
            using (Stream txtStream = txtEntry.Open())
            using (StreamWriter writer = new StreamWriter(txtStream, System.Text.Encoding.UTF8))
                await writer.WriteAsync(info);
        }
        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip", $"{fileName}.zip");
    }
    [HttpGet]
    public async Task<IActionResult> DownloadAll(int orderId)
    {
        int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        if (userId == 0) return RedirectToAction("Index", "AdminLogin");
        Order? order = await _orderRepository.GetByIdForUserAsync(orderId, userId);
        if (order == null) return RedirectToAction("Index");
        var closedItems = order.Items
            .Where(x => x.IsClosed && !string.IsNullOrEmpty(x.ZipFile))
            .ToList();
        if (!closedItems.Any())
        {
            TempData["Error"] = "İndirilecek teslim edilmiş ürün bulunamadı.";
            return RedirectToAction("Index");
        }
        using MemoryStream memoryStream = new MemoryStream();
        using (ZipArchive archive = new ZipArchive(
            memoryStream, ZipArchiveMode.Create, true))
        {
            int idx = 1;
            foreach (OrderItem item in closedItems)
            {
                string? charPw = _encryptionService.Decrypt(item.CharacterPw);
                string? mailPw = _encryptionService.Decrypt(item.CharacterMailPw);
                string? otpPw = _encryptionService.Decrypt(item.OtpPassword);
                string? info = $"Sipariş No    : {order.OrderNo}\r\n" +
                           $"Ürün          : {item.ProductName}\r\n" +
                           $"Server        : {item.ServerName}\r\n" +
                           $"Karakter ID   : {item.CharacterId}\r\n" +
                           $"Karakter Şifre: {charPw}\r\n" +
                           $"Karakter Mail : {item.CharacterMail}\r\n" +
                           $"Mail Şifre    : {mailPw}\r\n" +
                           $"OTP Kodu      : {item.OtpCode}\r\n" +
                           $"OTP Şifre     : {otpPw}\r\n";
                string? folderName = $"{item.CharacterId}-{idx:D2}";
                string? base64 = item.ZipFile.Contains(",")
                    ? item.ZipFile.Split(',')[1]
                    : item.ZipFile;
                byte[] zipBytes = Convert.FromBase64String(base64);
                ZipArchiveEntry zipEntry = archive.CreateEntry(
                    $"{folderName}/{folderName}.zip",
                    CompressionLevel.Fastest);
                using (Stream zipStream = zipEntry.Open())
                    await zipStream.WriteAsync(zipBytes);
                ZipArchiveEntry txtEntry = archive.CreateEntry(
                    $"{folderName}/{folderName}_bilgiler.txt",
                    CompressionLevel.Fastest);
                using (Stream txtStream = txtEntry.Open())
                using (StreamWriter writer = new StreamWriter(txtStream, System.Text.Encoding.UTF8))
                    await writer.WriteAsync(info);
                idx++;
            }
        }
        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip",
            $"{order.OrderNo}_tumu.zip");
    }
}