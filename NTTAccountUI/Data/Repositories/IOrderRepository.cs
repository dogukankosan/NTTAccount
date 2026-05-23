using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;
using System.Data;

namespace NTTAccountUI.Data.Repositories;

public interface IOrderRepository
{
    // IOrderRepository'e ekle
    Task<bool> ChangeStatusAsync(int orderId, byte status); // Manuel durum değiştir
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id); Task<bool> OpenItemAsync(int itemId, int orderId);

    Task<string> CreateAsync(Order order, List<OrderItem> items);
    Task<bool> UpdateAsync(Order order, List<OrderItem> items);
    Task<bool> DeleteAsync(int id);
    Task<bool> CloseItemAsync(int itemId, int orderId);   // satır kapat
    Task UpdateOrderStatusAsync(int orderId);              // durum yeniden hesapla

    // User
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetByIdForUserAsync(int id, int userId); // sadece kendi siparişi

    // Yardımcı
    Task<string> GenerateOrderNoAsync();
}

public class OrderRepository : IOrderRepository
{
    private readonly DapperContext _context;

    public OrderRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        const string orderSql = @"
        SELECT o.Id, o.OrderNo, o.UserId, o.Note, o.Status, o.CreatedAt, o.UpdatedAt,
               u.Email AS UserEmail
        FROM Orders o
        INNER JOIN Users u ON u.Id = o.UserId
        ORDER BY o.CreatedAt DESC";
         var orders = (await conn.QueryAsync<Order>(orderSql)).ToList();
        foreach (Order order in orders)
        {
            const string itemSql = @"
            SELECT oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                   oi.ServerName, oi.CharacterId, oi.CharacterPw,
                   oi.CharacterMail, oi.CharacterMailPw,
                   oi.OtpCode, oi.OtpPassword, oi.ZipFile, oi.IsClosed,
                   oi.CreatedAt, oi.UpdatedAt,
                   p.Name AS ProductName, p.Code AS ProductCode
            FROM OrderItems oi
            INNER JOIN Products p ON p.Id = oi.ProductId
            WHERE oi.OrderId = @OrderId";
            order.Items = (await conn.QueryAsync<OrderItem>(
                itemSql, new { OrderId = order.Id })).ToList();
        }
        return orders;
    }
    public async Task<Order?> GetByIdAsync(int id)
    {
        using var conn = _context.CreateConnection();
        const string orderSql = @"
            SELECT o.Id, o.OrderNo, o.UserId, o.Note, o.Status, o.CreatedAt, o.UpdatedAt,
                   u.Email AS UserEmail
            FROM Orders o
            INNER JOIN Users u ON u.Id = o.UserId
            WHERE o.Id = @Id";
        Order? order = await conn.QueryFirstOrDefaultAsync<Order>(orderSql, new { Id = id });
        if (order == null) return null;
        const string itemSql = @"
            SELECT oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                   oi.ServerName, oi.CharacterId, oi.CharacterPw,
                   oi.CharacterMail, oi.CharacterMailPw,
                   oi.OtpCode, oi.OtpPassword, oi.ZipFile, oi.IsClosed,
                   oi.CreatedAt, oi.UpdatedAt,
                   p.Name AS ProductName, p.Code AS ProductCode
            FROM OrderItems oi
            INNER JOIN Products p ON p.Id = oi.ProductId
            WHERE oi.OrderId = @OrderId";
        order.Items = (await conn.QueryAsync<OrderItem>(itemSql, new { OrderId = id })).ToList();
        return order;
    }
    public async Task<string> CreateAsync(Order order, List<OrderItem> items)
    {
        using var conn = _context.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // Sipariş no üret
            order.OrderNo = await GenerateOrderNoInternalAsync(conn, tx);
            // Stok kontrol
            foreach (OrderItem item in items)
            {
                var stock = await conn.ExecuteScalarAsync<int>(
                    "SELECT Stock FROM Products WHERE Id = @Id AND IsActive = 1",
                    new { Id = item.ProductId }, tx);

                if (stock < item.Quantity)
                    throw new InvalidOperationException($"Ürün ID {item.ProductId} için yeterli stok yok. Mevcut: {stock}");
            }
            // Sipariş ekle
            const string orderSql = @"
                INSERT INTO Orders (OrderNo, UserId, Note, Status)
                VALUES (@OrderNo, @UserId, @Note, 0);
                SELECT SCOPE_IDENTITY();";
            int orderId = await conn.ExecuteScalarAsync<int>(orderSql, new
            {
                order.OrderNo,
                order.UserId,
                order.Note
            }, tx);
            // Satırları ekle + stok düş
            foreach (OrderItem item in items)
            {
                const string itemSql = @"
                    INSERT INTO OrderItems
                        (OrderId, ProductId, Quantity, UnitPrice, ServerName,
                         CharacterId, CharacterPw, CharacterMail, CharacterMailPw,
                         OtpCode, OtpPassword, ZipFile)
                    VALUES
                        (@OrderId, @ProductId, @Quantity, @UnitPrice, @ServerName,
                         @CharacterId, @CharacterPw, @CharacterMail, @CharacterMailPw,
                         @OtpCode, @OtpPassword, @ZipFile)";
                await conn.ExecuteAsync(itemSql, new
                {
                    OrderId = orderId,
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.ServerName,
                    item.CharacterId,
                    item.CharacterPw,
                    item.CharacterMail,
                    item.CharacterMailPw,
                    item.OtpCode,
                    item.OtpPassword,
                    item.ZipFile
                }, tx);
                // Stoktan düş
                await conn.ExecuteAsync(
                    "UPDATE Products SET Stock = Stock - @Qty WHERE Id = @Id",
                    new { Qty = item.Quantity, Id = item.ProductId }, tx);
            }
            tx.Commit();
            return order.OrderNo;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // Repository'ye ekle
    public async Task<bool> OpenItemAsync(int itemId, int orderId)
    {
        using var conn = _context.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(@"
            UPDATE OrderItems SET IsClosed = 0, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id AND OrderId = @OrderId",
                new { Id = itemId, OrderId = orderId }, tx);
            await UpdateOrderStatusInternalAsync(conn, tx, orderId);
            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
    public async Task<bool> ChangeStatusAsync(int orderId, byte status)
    {
        using var conn = _context.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // Bekliyor → tüm satırları aç
            if (status == 0)
            {
                await conn.ExecuteAsync(@"
                UPDATE OrderItems SET IsClosed = 0, UpdatedAt = SYSUTCDATETIME()
                WHERE OrderId = @OrderId",
                    new { OrderId = orderId }, tx);
            }
            // Kapandı → tüm satırları kapat
            else if (status == 2)
            {
                await conn.ExecuteAsync(@"
                UPDATE OrderItems SET IsClosed = 1, UpdatedAt = SYSUTCDATETIME()
                WHERE OrderId = @OrderId",
                    new { OrderId = orderId }, tx);
            }
            // Kısmı Sevk → satırlar CloseSelectedItems'dan gelir, sadece status güncelle
            await conn.ExecuteAsync(@"
            UPDATE Orders SET Status = @Status, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id",
                new { Status = status, Id = orderId }, tx);
            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
    // DeleteAsync'i güncelle — sadece Bekliyor silinebilir
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _context.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            byte status = await conn.ExecuteScalarAsync<byte>(
                "SELECT Status FROM Orders WHERE Id = @Id", new { Id = id }, tx);
            if (status != 0)
                throw new InvalidOperationException(
                    "Sadece 'Bekliyor' durumundaki siparişler silinebilir.");
            // Stok iade — sadece açık satırlar
            var items = await conn.QueryAsync<OrderItem>(
                "SELECT ProductId, Quantity FROM OrderItems WHERE OrderId = @OrderId",
                new { OrderId = id }, tx);
            foreach (OrderItem item in items)
            {
                await conn.ExecuteAsync(
                    "UPDATE Products SET Stock = Stock + @Qty WHERE Id = @Id",
                    new { Qty = item.Quantity, Id = item.ProductId }, tx);
            }
            await conn.ExecuteAsync(
                "DELETE FROM Orders WHERE Id = @Id", new { Id = id }, tx);
            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
    // UpdateAsync'i güncelle — Kapandı güncellenemez, kapanmış satırlar değiştirilemez
    public async Task<bool> UpdateAsync(Order order, List<OrderItem> items)
    {
        using var conn = _context.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            byte status = await conn.ExecuteScalarAsync<byte>(
                "SELECT Status FROM Orders WHERE Id = @Id", new { order.Id }, tx);
            if (status == 2)
                throw new InvalidOperationException("Kapanmış sipariş güncellenemez.");
            // Mevcut satırları al
            var existingItems = (await conn.QueryAsync<OrderItem>(
                "SELECT Id, ProductId, Quantity, IsClosed FROM OrderItems WHERE OrderId = @OrderId",
                new { OrderId = order.Id }, tx)).ToList();
            // Stok iadesi — sadece açık satırlar için
            foreach (OrderItem ex in existingItems.Where(i => !i.IsClosed))
            {
                await conn.ExecuteAsync(
                    "UPDATE Products SET Stock = Stock + @Qty WHERE Id = @Id",
                    new { Qty = ex.Quantity, Id = ex.ProductId }, tx);
            }
            // Stok kontrolü — yeni satırlar için
            foreach (OrderItem item in items)
            {
                int stock = await conn.ExecuteScalarAsync<int>(
                    "SELECT Stock FROM Products WHERE Id = @Id AND IsActive = 1",
                    new { Id = item.ProductId }, tx);
                if (stock < item.Quantity)
                    throw new InvalidOperationException(
                        $"Ürün ID {item.ProductId} için yeterli stok yok. Mevcut: {stock}");
            }
            // Sadece açık satırları sil — kapanmış satırlara dokunma
            await conn.ExecuteAsync(
                "DELETE FROM OrderItems WHERE OrderId = @OrderId AND IsClosed = 0",
                new { OrderId = order.Id }, tx);
            // Yeni satırları ekle + stok düş
            foreach (OrderItem item in items)
            {
                const string itemSql = @"
                INSERT INTO OrderItems
                    (OrderId, ProductId, Quantity, UnitPrice, ServerName,
                     CharacterId, CharacterPw, CharacterMail, CharacterMailPw,
                     OtpCode, OtpPassword, ZipFile)
                VALUES
                    (@OrderId, @ProductId, @Quantity, @UnitPrice, @ServerName,
                     @CharacterId, @CharacterPw, @CharacterMail, @CharacterMailPw,
                     @OtpCode, @OtpPassword, @ZipFile)";
                await conn.ExecuteAsync(itemSql, new
                {
                    OrderId = order.Id,
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.ServerName,
                    item.CharacterId,
                    item.CharacterPw,
                    item.CharacterMail,
                    item.CharacterMailPw,
                    item.OtpCode,
                    item.OtpPassword,
                    item.ZipFile
                }, tx);
                await conn.ExecuteAsync(
                    "UPDATE Products SET Stock = Stock - @Qty WHERE Id = @Id",
                    new { Qty = item.Quantity, Id = item.ProductId }, tx);
            }
            await conn.ExecuteAsync(@"
    UPDATE Orders SET 
        Note      = @Note,
        UserId    = @UserId,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id",
         new { order.Note, order.UserId, order.Id }, tx);
            await UpdateOrderStatusInternalAsync(conn, tx, order.Id);
            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
    public async Task<bool> CloseItemAsync(int itemId, int orderId)
    {
        using var conn = _context.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(@"
                UPDATE OrderItems SET IsClosed = 1, UpdatedAt = SYSUTCDATETIME()
                WHERE Id = @Id AND OrderId = @OrderId",
                new { Id = itemId, OrderId = orderId }, tx);
            await UpdateOrderStatusInternalAsync(conn, tx, orderId);
            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId)
    {
        using var conn = _context.CreateConnection();
        await UpdateOrderStatusInternalAsync(conn, null, orderId);
    }

    // User — sadece kendi siparişleri, sadece kapanan satırlar gelir
    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        using var conn = _context.CreateConnection();
        const string orderSql = @"
            SELECT o.Id, o.OrderNo, o.Note, o.Status, o.CreatedAt, o.UpdatedAt
            FROM Orders o
            WHERE o.UserId = @UserId
            ORDER BY o.CreatedAt DESC";
        var orders = (await conn.QueryAsync<Order>(orderSql, new { UserId = userId })).ToList();
        foreach (Order order in orders)
        {
            const string itemSql = @"
                SELECT oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                       oi.ServerName, oi.CharacterId, oi.CharacterPw,
                       oi.CharacterMail, oi.CharacterMailPw,
                       oi.OtpCode, oi.OtpPassword, oi.ZipFile, oi.IsClosed,
                       p.Name AS ProductName, p.Code AS ProductCode
                FROM OrderItems oi
                INNER JOIN Products p ON p.Id = oi.ProductId
                WHERE oi.OrderId = @OrderId AND oi.IsClosed = 1
                ORDER BY oi.CreatedAt";
            order.Items = (await conn.QueryAsync<OrderItem>(
                itemSql, new { OrderId = order.Id })).ToList();
        }
        return orders;
    }

    public async Task<Order?> GetByIdForUserAsync(int id, int userId)
    {
        using var conn = _context.CreateConnection();
        const string orderSql = @"
            SELECT o.Id, o.OrderNo, o.Note, o.Status, o.CreatedAt, o.UpdatedAt
            FROM Orders o
            WHERE o.Id = @Id AND o.UserId = @UserId";
        Order? order = await conn.QueryFirstOrDefaultAsync<Order>(
            orderSql, new { Id = id, UserId = userId });
        if (order == null) return null;
        const string itemSql = @"
            SELECT oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                   oi.ServerName, oi.CharacterId, oi.CharacterPw,
                   oi.CharacterMail, oi.CharacterMailPw,
                   oi.OtpCode, oi.OtpPassword, oi.ZipFile, oi.IsClosed,
                   p.Name AS ProductName, p.Code AS ProductCode
            FROM OrderItems oi
            INNER JOIN Products p ON p.Id = oi.ProductId
            WHERE oi.OrderId = @OrderId AND oi.IsClosed = 1";
        order.Items = (await conn.QueryAsync<OrderItem>(
            itemSql, new { OrderId = id })).ToList();
        return order;
    }

    public async Task<string> GenerateOrderNoAsync()
    {
        using var conn = _context.CreateConnection();
        return await GenerateOrderNoInternalAsync(conn, null);
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static async Task<string> GenerateOrderNoInternalAsync(
        IDbConnection conn,
        IDbTransaction? tx)
    {
        string date = DateTime.UtcNow.ToString("yyyyMMdd");
        string prefix = $"ORD-{date}-";
        string? last = await conn.ExecuteScalarAsync<string?>(@"
            SELECT TOP 1 OrderNo FROM Orders
            WHERE OrderNo LIKE @Prefix + '%'
            ORDER BY Id DESC",
            new { Prefix = prefix }, tx);
        int seq = 1;
        if (last != null)
        {
            string[] parts = last.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int n))
                seq = n + 1;
        }
        return $"{prefix}{seq:D4}";
    }
    private static async Task UpdateOrderStatusInternalAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        int orderId)
    {
        int total = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM OrderItems WHERE OrderId = @Id",
            new { Id = orderId }, tx);

        int closed = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM OrderItems WHERE OrderId = @Id AND IsClosed = 1",
            new { Id = orderId }, tx);
        byte status = (total == 0 || closed == 0) ? (byte)0
                    : closed == total ? (byte)2
                    : (byte)1;
        await conn.ExecuteAsync(@"
            UPDATE Orders SET Status = @Status, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id",
            new { Status = status, Id = orderId }, tx);
    }
}