using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Secrets;

namespace Alien.Common.Config;

public enum KeyVaultAuthType
{
    ClientSecret,
    CertificateFile,
    CertificateStore
}


public class KeyVaultOptions
{
    public string VaultUri { get; set; } = default;
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(30);
    public string SqliteConnectionString { get; set; } = "Data Source=secretstore.db";
    public string EncryptionKey { get; set; } = default;

    public KeyVaultAuthType AuthType { get; set; } = KeyVaultAuthType.ClientSecret;

    // Certificate auth
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }

    // Client Secret
    public string? ClientSecret { get; set; }

    // PFX File
    public string? CertificatePath { get; set; }
    public string? CertificatePassword { get; set; }

    // Certificate Store
    public string? CertificateThumbprint { get; set; }
    public StoreLocation StoreLocation { get; set; } = StoreLocation.LocalMachine;

    // Proxy Options
    public SecretClientOptions? options { get; set; }
}
