using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;

namespace Alien.Common.Config;

public static class AzureCredentialFactory
{
    public static TokenCredential Create(KeyVaultOptions options)
    {
        if(string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.TenantId))
        {
            throw new ArgumentException("ClientId and TenantId must be provided.");
        }

        return options.AuthType switch
        {
            KeyVaultAuthType.CertificateStore => CreateFromStore(options),
            KeyVaultAuthType.ClientSecret => CreateClientSecret(options),
            KeyVaultAuthType.CertificateFile => CreateFromFile(options),
            _ => throw new NotSupportedException($"Unsupported authentication type: {options.AuthType}")
        };
    }

    private static TokenCredential CreateClientSecret(KeyVaultOptions options)
    {
        if(string.IsNullOrEmpty(options.ClientSecret))
            throw new ArgumentException("ClientSecret is required");

        return new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
    }

    private static TokenCredential CreateFromFile(KeyVaultOptions options)
    {
        if(string.IsNullOrEmpty(options.CertificatePath))
            throw new ArgumentException("CertificatePath is required");

        var cert = new X509Certificate2(options.CertificatePath, options.CertificatePassword);

        return new ClientCertificateCredential(options.TenantId, options.ClientId, cert);
    }

    private static TokenCredential CreateFromStore(KeyVaultOptions options)
    {
        if (string.IsNullOrEmpty(options.CertificateThumbprint))
            throw new ArgumentException("CertificateThumbprint is required");

        using var store = new X509Store(options.StoreLocation);
        store.Open(OpenFlags.ReadOnly);

        var certs = store.Certificates.Find(X509FindType.FindByThumbprint, options.CertificateThumbprint, validOnly: false);

        if (certs.Count == 0)
            throw new Exception("Certificate not found");

        return new ClientCertificateCredential(options.TenantId, options.ClientId, certs[0]);
    }
}
