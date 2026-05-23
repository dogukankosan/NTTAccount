using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.Entities;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Security;
using NTTAccountUI.Services;
using System.IO.Compression;
using System.Text;

namespace NTTAccountUI.Controllers;

public class AdminOrderController : AdminBaseController
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogService _logService;
    private readonly IEncryptionService _encryptionService;

    public AdminOrderController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogService logService,
        IEncryptionService encryptionService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logService = logService;
        _encryptionService = encryptionService;
    }
    // ─── AdminOrderController'a eklenecek action'lar ──────────────────────────
    [HttpGet]
    public async Task<IActionResult> AdminDownloadZip(int itemId, int orderId)
    {
        Order? order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return RedirectToAction("Index");
        OrderItem? item = order.Items.FirstOrDefault(x => x.Id == itemId);
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
            // Orijinal ZIP
            string? base64 = item.ZipFile.Contains(",")
                ? item.ZipFile.Split(',')[1]
                : item.ZipFile;
            byte[] zipBytes = Convert.FromBase64String(base64);
            ZipArchiveEntry zipEntry = archive.CreateEntry(
                $"{fileName}.zip",
                CompressionLevel.Fastest);
            using (Stream zipStream = zipEntry.Open())
                await zipStream.WriteAsync(zipBytes);
            // Bilgi TXT
            ZipArchiveEntry txtEntry = archive.CreateEntry(
                $"{fileName}_bilgiler.txt",
               CompressionLevel.Fastest);
            using (Stream txtStream = txtEntry.Open())
            using (StreamWriter writer = new StreamWriter(txtStream, Encoding.UTF8))
                await writer.WriteAsync(info);
        }
        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip", $"{fileName}.zip");
    }
    [HttpGet]
    public async Task<IActionResult> AdminDownloadAll(int orderId)
    {
        Order? order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return RedirectToAction("Index");
        var itemsWithZip = order.Items
            .Where(x => !string.IsNullOrEmpty(x.ZipFile))
            .ToList();
        if (!itemsWithZip.Any())
        {
            TempData["Error"] = "İndirilecek ZIP dosyası bulunamadı.";
            return RedirectToAction("Index");
        }
        using MemoryStream memoryStream = new MemoryStream();
        using (ZipArchive archive = new ZipArchive(
            memoryStream, ZipArchiveMode.Create, true))
        {
            int idx = 1;
            foreach (OrderItem item in itemsWithZip)
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
                // Orijinal ZIP
                string? base64 = item.ZipFile.Contains(",")
                    ? item.ZipFile.Split(',')[1]
                    : item.ZipFile;
                byte[] zipBytes = Convert.FromBase64String(base64);
                ZipArchiveEntry zipEntry = archive.CreateEntry(
                    $"{folderName}/{folderName}.zip",
                    System.IO.Compression.CompressionLevel.Fastest);
                using (Stream zipStream = zipEntry.Open())
                    await zipStream.WriteAsync(zipBytes);
                // Bilgi TXT
                ZipArchiveEntry txtEntry = archive.CreateEntry(
                    $"{folderName}/{folderName}_bilgiler.txt",
                    System.IO.Compression.CompressionLevel.Fastest);
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
    // CloseAllItems — güncelle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseAllItems(int orderId)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            Order? order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) { TempData["Error"] = "Sipariş bulunamadı."; return RedirectToAction("Index"); }
            foreach (OrderItem item in order.Items.Where(i => !i.IsClosed))
                await _orderRepository.CloseItemAsync(item.Id, orderId);
            await _logService.LogInfoAsync($"Tüm satırlar kapatıldı: #{orderId}", "AdminOrder", adminId);
            TempData["Success"] = "Tüm satırlar kapatıldı.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.CloseAllItems", adminId);
            TempData["Error"] = "Hata oluştu.";
        }
        return RedirectToAction("Index");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseSelectedItems(int orderId, List<int> itemIds)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (itemIds == null || !itemIds.Any())
            {
                TempData["Error"] = "Hiç satır seçilmedi.";
                return RedirectToAction("Index");
            }
            foreach (int itemId in itemIds)
                await _orderRepository.CloseItemAsync(itemId, orderId);
            await _logService.LogInfoAsync(
                $"Seçili satırlar kapatıldı: #{orderId}", "AdminOrder", adminId);
            TempData["Success"] = $"{itemIds.Count} satır kapatıldı.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.CloseSelectedItems", adminId);
            TempData["Error"] = "Hata oluştu.";
        }
        return RedirectToAction("Index");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int orderId, byte status)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            Order? order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index");
            }
            // Kapandı → Bekliyor veya Kısmı Sevk'e geçerken satırları geri aç
            if (order.Status == 2 && status < 2)
            {
                await _orderRepository.ChangeStatusAsync(orderId, status);
                await _logService.LogInfoAsync(
                    $"Sipariş durumu geri alındı: #{orderId} → {status}",
                    "AdminOrder", adminId);
                TempData["Success"] = "Sipariş durumu güncellendi.";
                return RedirectToAction("Index");
            }
            // Normal durum geçişi
            await _orderRepository.ChangeStatusAsync(orderId, status);
            await _logService.LogInfoAsync(
                $"Sipariş durumu değiştirildi: #{orderId} → {status}",
                "AdminOrder", adminId);
            TempData["Success"] = "Sipariş durumu güncellendi.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.ChangeStatus", adminId);
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Index");
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _orderRepository.GetAllAsync();
        foreach (Order order in orders)
            foreach (OrderItem item in order.Items)
            {
                item.CharacterPw = _encryptionService.Decrypt(item.CharacterPw);
                item.CharacterMailPw = _encryptionService.Decrypt(item.CharacterMailPw);
                item.OtpPassword = _encryptionService.Decrypt(item.OtpPassword);
            }
        return View(orders);
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadProducts();
        await LoadUsers();
        return View(new OrderViewModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (model.Items == null || !model.Items.Any())
            {
                TempData["Error"] = "En az bir sipariş satırı eklenmelidir.";
                await LoadProducts();
                await LoadUsers();
                return View(model);
            }
            foreach (OrderItemViewModel item in model.Items)
            {
                if (string.IsNullOrEmpty(item.ZipBase64) && string.IsNullOrEmpty(item.ZipFile))
                {
                    TempData["Error"] = "Her satır için ZIP dosyası zorunludur.";
                    await LoadProducts();
                    await LoadUsers();
                    return View(model);
                }
            }
            Order order = new Order
            {
                UserId = model.UserId,
                Note = model.Note
            };
            var items = model.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = decimal.TryParse(
                                      (i.UnitPriceRaw ?? i.UnitPrice.ToString())
                                          .Replace(',', '.'),
                                      System.Globalization.NumberStyles.Any,
                                      System.Globalization.CultureInfo.InvariantCulture,
                                      out var price) ? price : i.UnitPrice,
                ServerName = i.ServerName,
                CharacterId = i.CharacterId,
                CharacterPw = _encryptionService.Encrypt(i.CharacterPw),
                CharacterMail = i.CharacterMail,
                CharacterMailPw = _encryptionService.Encrypt(i.CharacterMailPw),
                OtpCode = i.OtpCode,
                OtpPassword = _encryptionService.Encrypt(i.OtpPassword),
                ZipFile = i.ZipBase64 ?? i.ZipFile ?? string.Empty
            }).ToList();
            string orderNo = await _orderRepository.CreateAsync(order, items);
            await _logService.LogInfoAsync($"Sipariş oluşturuldu: {orderNo}", "AdminOrder", adminId);
            TempData["Success"] = $"Sipariş oluşturuldu: {orderNo}";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.Create", adminId);
            TempData["Error"] = "Hata oluştu.";
        }
        await LoadProducts();
        await LoadUsers();
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        Order? order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return RedirectToAction("Index");
        if (order.Status == 2)
        {
            TempData["Error"] = "Kapanmış sipariş düzenlenemez.";
            return RedirectToAction("Index");
        }
        await LoadProducts();
        await LoadUsers();
        return View(new OrderViewModel
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            UserId = order.UserId,
            Note = order.Note,
            Status = order.Status,
            CreatedAt = order.CreatedAt, // ✅ ekle
            Items = order.Items.Select(i => new OrderItemViewModel
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductCode = i.ProductCode,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                ServerName = i.ServerName,
                CharacterId = i.CharacterId,
                CharacterMailPw = _encryptionService.Decrypt(i.CharacterMailPw), // ✅ ekle
                CharacterPw = _encryptionService.Decrypt(i.CharacterPw),
                CharacterMail = i.CharacterMail,
                OtpCode = i.OtpCode,
                OtpPassword = _encryptionService.Decrypt(i.OtpPassword),
                ZipFile = i.ZipFile,
                IsClosed = i.IsClosed
            }).ToList()
        });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrderViewModel model)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        // DEBUG — silelim sonra

        try
        {
            // Tüm items null ise hata ver — ama boş liste de olabilir (kapanmış satırlar var)
            if (model.Items == null)
            {
                TempData["Error"] = "En az bir sipariş satırı eklenmelidir.";
                await LoadProducts();
                await LoadUsers();
                return View(model);
            }
           // Açık satırlar
            var openItems = model.Items.Where(i => !i.IsClosed).ToList();
            var closedItems = model.Items.Where(i => i.IsClosed).ToList();
            // Sadece hiç satır yoksa hata ver
            // Kapanmış satır varsa veya yeni açık satır varsa devam et
            if (!openItems.Any() && !closedItems.Any())
            {
                TempData["Error"] = "En az bir sipariş satırı eklenmelidir.";
                await LoadProducts();
                await LoadUsers();
                return View(model);
            }
            // Açık satır yoksa ve kapalı satır varsa — tamam, sadece not güncelle
            // Açık satır varsa ZIP kontrolü yap
            foreach (OrderItemViewModel item in openItems)
            {
                if (string.IsNullOrEmpty(item.ZipBase64) && string.IsNullOrEmpty(item.ZipFile))
                {
                    TempData["Error"] = "Her satır için ZIP dosyası zorunludur.";
                    await LoadProducts();
                    await LoadUsers();
                    return View(model);
                }
            }
            Order order = new Order
            {
                Id = model.Id,
                UserId = model.UserId,
                Note = model.Note
            };
            // Sadece açık satırları repository'e gönder
            var items = openItems.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.FinalUnitPrice, // ✅ değiştir
                ServerName = i.ServerName,
                CharacterId = i.CharacterId,
                CharacterPw = _encryptionService.Encrypt(i.CharacterPw),
                CharacterMail = i.CharacterMail,
                CharacterMailPw = _encryptionService.Encrypt(i.CharacterMailPw),
                OtpCode = i.OtpCode,
                OtpPassword = _encryptionService.Encrypt(i.OtpPassword),
                ZipFile = i.ZipBase64 ?? i.ZipFile ?? string.Empty
            }).ToList();
            await _orderRepository.UpdateAsync(order, items);
            await _logService.LogInfoAsync($"Sipariş güncellendi: #{model.Id}", "AdminOrder", adminId);
            TempData["Success"] = "Sipariş güncellendi.";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.Edit", adminId);
            TempData["Error"] = "Hata oluştu.";
        }
        await LoadProducts();
        await LoadUsers();
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleItems(int orderId, List<int> itemIds)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (itemIds == null || !itemIds.Any())
            {
                TempData["Error"] = "Hiç satır seçilmedi.";
                return RedirectToAction("Index");
            }
            Order? order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return RedirectToAction("Index");
            foreach (int itemId in itemIds)
            {
                OrderItem? item = order.Items.FirstOrDefault(i => i.Id == itemId);
                if (item == null) continue;
                if (item.IsClosed)
                    // Kapanmışı aç
                    await _orderRepository.OpenItemAsync(itemId, orderId);
                else
                    // Açığı kapat
                    await _orderRepository.CloseItemAsync(itemId, orderId);
            }
            await _logService.LogInfoAsync($"Satır durumları güncellendi: #{orderId}", "AdminOrder", adminId);
            TempData["Success"] = "Satır durumları güncellendi.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.ToggleItems", adminId);
            TempData["Error"] = "Hata oluştu.";
        }
        return RedirectToAction("Index");
    }
    // Delete — güncelle (sadece Bekliyor silinebilir)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            await _orderRepository.DeleteAsync(id);
            await _logService.LogInfoAsync($"Sipariş silindi: #{id}", "AdminOrder", adminId);
            TempData["Success"] = "Sipariş silindi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.Delete", adminId);
            TempData["Error"] = "Silme başarısız.";
        }
        return RedirectToAction("Index");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseItem(int itemId, int orderId)
    {
        int adminId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            await _orderRepository.CloseItemAsync(itemId, orderId);
            await _logService.LogInfoAsync(
                $"Sipariş satırı kapatıldı: #{itemId}", "AdminOrder", adminId);
            TempData["Success"] = "Satır kapatıldı.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "AdminOrder.CloseItem", adminId);
            TempData["Error"] = "Hata oluştu.";
        }
        return RedirectToAction("Edit", new { id = orderId });
    }
    // ─── Private Helpers ──────────────────────────────────────────────────
    private async Task LoadProducts()
        => ViewBag.Products = await _productRepository.GetActiveAsync();
    private async Task LoadUsers()
    {
        var all = await _userRepository.GetAllAsync();
        ViewBag.Users = all.Where(u => u.IsActive);
    }
}