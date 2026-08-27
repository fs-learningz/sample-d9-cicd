using Dapper;
using Microsoft.Data.Sqlite;
using Note.Application.ExternalServices.Persistence;

namespace Note.Infrastructure.Persistence;

public class QueryHmacNonce : IQueryHmacNonce
{
    private readonly SqliteConnection _sqliteConnection;

    public QueryHmacNonce(SqliteConnection sqliteConnection)
    {
        _sqliteConnection = sqliteConnection;
    }

    public async Task<bool> TryClaim(string nonce)
    {
        const string command = """
                               insert or ignore into hmac_nonce (nonce, created_at)
                               values (@Nonce, @CreatedAt)
                               """;

        var claimed = await _sqliteConnection.ExecuteAsync(command,
            new
            {
                Nonce = nonce,
                CreatedAt = DateTime.UtcNow
            });

        return claimed is 1;
    }
}