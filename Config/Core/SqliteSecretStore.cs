using Alien.Common.Utility;
using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Alien.Common.Config;

public class SqliteSecretStore
{
    private readonly string _conn;
    private readonly string _key;

    public SqliteSecretStore(string connectionString, string key)
    {
        _conn = connectionString;
        _key = key;

        Init();
    }

    private void Init()
    {
        using var connection = new SqliteConnection(_conn);
        connection.Execute("""
        CREATE TABLE IF NOT EXISTS secrets (
            name TEXT PRIMARY KEY,
            value TEXT NOT NULL,
            updated_at TEXT NOT NULL
        )
        """);
    }

    public (string value, DateTime updateAt)? Get(string name)
    {
        using var db = new SqliteConnection(_conn);
        var row = db.QueryFirstOrDefault<dynamic>("SELECT value, updated_at FROM secrets WHERE name = @name", new { name });
        if (row == null) return null;
        var crypto = new AesGcmCrypto();
        var decrypted = crypto.Decrypt(Encoding.UTF8.GetBytes(row.value), Encoding.UTF8.GetBytes(_key));
        return (decrypted, DateTime.Parse(row.updated_at));
    }

    public void Set(string name, string value)
    {
        using var db = new SqliteConnection(_conn);
        var crypto = new AesGcmCrypto();
        var encrypted = crypto.Encrypt(value, Encoding.UTF8.GetBytes(_key));

        db.Execute("""
            INSERT INTO secrets (name, value, updated_at) 
            VALUES (@name, @value, @updatedAt)
            ON CONFLICT(name) DO UPDATE SET 
                value = excluded.value, 
                updated_at = excluded.updated_at 
        """, 
        new
        {
            name,
            value = encrypted,
            updatedAt = DateTime.UtcNow
        });
    }
}
