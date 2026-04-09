using Azure.Security.KeyVault.Secrets;
using System.Collections.Concurrent;
using Alien.Common.Utility;
using System.Text;

namespace Alien.Common.Config;

public class KeyVaultSecretProvider : ISecretProvider
{
    private readonly SecretClient _client;
    private readonly KeyVaultOptions _options;
    private readonly SqliteSecretStore _sqlite;
    private readonly ConcurrentDictionary<string, (string value, DateTime expiry)> _cache = new();

    public KeyVaultSecretProvider(KeyVaultOptions options)
    {
        _options = options;
        _sqlite = new SqliteSecretStore(options.SqliteConnectionString, options.EncryptionKey);
        var credential = AzureCredentialFactory.Create(options);
        _client = new SecretClient(new Uri(options.VaultUri), credential, options.options);
    }

    public async Task<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        // 1. Memory cache
        if (_cache.TryGetValue(name, out var cacheEntry) && cacheEntry.expiry > DateTime.UtcNow)
        {
            return cacheEntry.value;
        }

        // 2. Key Vault
        try
        {
            var response = await _client.GetSecretAsync(name, cancellationToken: cancellationToken);
            var value = response.Value.Value;
            
            // Cache the secret value with an expiry time
            _cache[name] = (value, DateTime.UtcNow.Add(_options.CacheDuration));
            _sqlite.Set(name, value);
            return value;
        }
        catch
        {
            // fall back: SQLite cache
            var local = _sqlite.Get(name);
            if(local.Value.value == null)
            {
                return "";
            }
            _cache[name] = (local.Value.value, DateTime.UtcNow.Add(_options.CacheDuration));
            return local.Value.value;
        }
    }

    public async Task<string?> GetSecretAsync(string name, byte[] key, CancellationToken cancellationToken = default)
    {
        string secret = await GetSecretAsync(name, cancellationToken);
        if (secret == null)
        {
            return "";
        }
        AesGcmCrypto crypto = new AesGcmCrypto();
        return crypto.Decrypt(Encoding.UTF8.GetBytes(secret), key);
    }
}
