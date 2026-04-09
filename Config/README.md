# Alien.Common.Config

`Alien.Common.Config` 提供兩類能力：

1. `AppConfig`: 快速讀取 `appsettings.json`。
2. `ISecretProvider` / `KeyVaultSecretProvider`: 從 Azure Key Vault 讀取機密，並支援記憶體快取與 SQLite 本地快取。

## 安裝

```bash
dotnet add package Alien.Common.Config
```

## 功能概覽

### 1) 讀取設定檔 (`AppConfig`)

```csharp
using Alien.Common.Config;

var appConfig = new AppConfig("appsettings.json");
string? value = appConfig.Configuration["MySetting:SubKey"];
Console.WriteLine(value);
```

### 2) 讀取 Key Vault 機密 (`ISecretProvider`)

- 先查記憶體快取。
- 快取失效時，向 Azure Key Vault 讀取。
- 讀取成功後寫入 SQLite 本地快取。
- 若 Key Vault 暫時失敗，會嘗試讀取 SQLite 快取作為 fallback。

## DI 注入範例

下面以 `ClientSecret` 驗證模式為例。

```csharp
using Alien.Common.Config.Abstraction;
using Alien.Common.Config.Extensions;
using Alien.Common.Config.Options;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddKeyVaultSecretProvider(options =>
{
    options.VaultUri = "https://your-vault-name.vault.azure.net/";
    options.AuthType = KeyVaultAuthType.ClientSecret;

    options.TenantId = "<tenant-id>";
    options.ClientId = "<client-id>";
    options.ClientSecret = "<client-secret>";

    options.CacheDuration = TimeSpan.FromMinutes(30);
    options.SqliteConnectionString = "Data Source=secretstore.db";
    options.EncryptionKey = "your-local-cache-encryption-key";
});

using var provider = services.BuildServiceProvider();
var secretProvider = provider.GetRequiredService<ISecretProvider>();

string? secret = await secretProvider.GetSecretAsync("MySecretName");
Console.WriteLine(secret);
```

## 一般使用（不透過 DI）範例

```csharp
using Alien.Common.Config.Abstraction;
using Alien.Common.Config.Core;
using Alien.Common.Config.Options;

ISecretProvider secretProvider = new KeyVaultSecretProvider(new KeyVaultOptions
{
    VaultUri = "https://your-vault-name.vault.azure.net/",
    AuthType = KeyVaultAuthType.ClientSecret,
    TenantId = "<tenant-id>",
    ClientId = "<client-id>",
    ClientSecret = "<client-secret>",
    CacheDuration = TimeSpan.FromMinutes(30),
    SqliteConnectionString = "Data Source=secretstore.db",
    EncryptionKey = "your-local-cache-encryption-key"
});

string? secret = await secretProvider.GetSecretAsync("MySecretName");
Console.WriteLine(secret);
```

## 使用解密 key 的多載範例

`ISecretProvider` 另提供：

```csharp
Task<string?> GetSecretAsync(string name, byte[] key, CancellationToken cancellationToken = default)
```

用法：

```csharp
byte[] decryptKey = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");
string? plainText = await secretProvider.GetSecretAsync("EncryptedSecretName", decryptKey);
```

> 注意：`key` 長度需符合 AES 規範（16/24/32 bytes）。

## 認證模式說明

`KeyVaultOptions.AuthType` 支援：

1. `KeyVaultAuthType.ClientSecret`
2. `KeyVaultAuthType.CertificateFile`
3. `KeyVaultAuthType.CertificateStore`

必要欄位：

- 共通：`VaultUri`, `TenantId`, `ClientId`, `AuthType`, `EncryptionKey`
- `ClientSecret` 模式：`ClientSecret`
- `CertificateFile` 模式：`CertificatePath`（可選 `CertificatePassword`）
- `CertificateStore` 模式：`CertificateThumbprint`（可搭配 `StoreLocation`）