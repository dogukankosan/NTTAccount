using Dapper;
using NTTAccountUI.Data;
using NTTAccountUI.Models.Entities;

namespace NTTAccountUI.Data.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();         // Admin — hepsi
    Task<IEnumerable<Product>> GetActiveAsync();      // User — sadece aktifler
    Task<Product?> GetByIdAsync(int id);
    Task<int> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> CodeExistsAsync(string code, int excludeId = 0);
    Task<bool> HasOrderAsync(int productId);          // Siparişe bağlı mı?
}

public class ProductRepository : IProductRepository
{
    private readonly DapperContext _context;

    public ProductRepository(DapperContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string sql = @"
            SELECT p.Id, p.Code, p.Name, p.Description, p.Price,
                   p.Stock, p.IsActive, p.CreatedBy, p.CreatedAt, p.UpdatedAt,
                   u.Email AS CreatedByEmail
            FROM Products p
            INNER JOIN Users u ON u.Id = p.CreatedBy
            ORDER BY p.CreatedAt DESC";
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<Product>(sql);
    }
    public async Task<IEnumerable<Product>> GetActiveAsync()
    {
        const string sql = @"
            SELECT Id, Code, Name, Description, Price, Stock, CreatedAt
            FROM Products
            WHERE IsActive = 1
            ORDER BY CreatedAt DESC";
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<Product>(sql);
    }
    public async Task<Product?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT p.Id, p.Code, p.Name, p.Description, p.Price,
                   p.Stock, p.IsActive, p.CreatedBy, p.CreatedAt, p.UpdatedAt,
                   u.Email AS CreatedByEmail
            FROM Products p
            INNER JOIN Users u ON u.Id = p.CreatedBy
            WHERE p.Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }
    public async Task<int> CreateAsync(Product product)
    {
        const string sql = @"
            INSERT INTO Products (Code, Name, Description, Price, Stock, IsActive, CreatedBy)
            VALUES (@Code, @Name, @Description, @Price, @Stock, @IsActive, @CreatedBy);
            SELECT SCOPE_IDENTITY();";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, product);
    }
    public async Task<bool> UpdateAsync(Product product)
    {
        const string sql = @"
            UPDATE Products SET
                Code        = @Code,
                Name        = @Name,
                Description = @Description,
                Price       = @Price,
                Stock       = @Stock,
                IsActive    = @IsActive,
                UpdatedAt   = SYSUTCDATETIME()
            WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, product) > 0;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
    public async Task<bool> CodeExistsAsync(string code, int excludeId = 0)
    {
        const string sql = @"
        SELECT COUNT(1) FROM Products
        WHERE Code = @Code AND Id != @ExcludeId";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new { Code = code, ExcludeId = excludeId }) > 0;
    }
    public async Task<bool> HasOrderAsync(int productId)
    {
        const string sql = @"
        SELECT COUNT(1) FROM OrderItems WHERE ProductId = @ProductId";
        using var conn = _context.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new { ProductId = productId }) > 0;
    }
}