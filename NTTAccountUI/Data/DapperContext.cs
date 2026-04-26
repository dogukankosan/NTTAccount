using System.Data;
using Microsoft.Data.SqlClient;

namespace NTTAccountUI.Data;

public class DapperContext
{
    private readonly string _connectionString;
    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("NttAccountDB")
            ?? throw new InvalidOperationException("NttAccountDB connection string bulunamadı.");
    }
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}