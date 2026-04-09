using Alien.Common.Config;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using TestMyLib.Options;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Azure.Security.KeyVault.Secrets;

namespace TestMyLib;

[TestFixture]
public class KeyVaultTest
{
    private IConfiguration _configuration;
    private ISecretProvider _secretProvider;


    [OneTimeSetUp]
    public void InitData()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<KeyVaultTest>()
            .Build();
        ProxyOption proxyOption = new ProxyOption(_configuration["Alien:Proxy:IP"], _configuration["Alien:Proxy:Account"], _configuration["Alien:Proxy:Password"]);
        var handler = new HttpClientHandler()
        {
            Proxy = new WebProxy(proxyOption.IP)
            {
                Credentials = new NetworkCredential(proxyOption.Account, proxyOption.Password)
            },
            UseProxy = true,
            PreAuthenticate = true,
            UseDefaultCredentials = false
        };
        var httpCLient = new HttpClient(handler);
        SecretClientOptions myProxy = new SecretClientOptions()
        {
            Transport = new Azure.Core.Pipeline.HttpClientTransport(httpCLient)
        };

        _secretProvider = new KeyVaultSecretProvider(new KeyVaultOptions
        {
            VaultUri = _configuration["Alien:AzureKeyVault:URI"],
            AuthType = KeyVaultAuthType.ClientSecret,
            TenantId = _configuration["Alien:AzureKeyVault:TenantID"],
            ClientId = _configuration["Alien:AzureKeyVault:ClientID"],
            ClientSecret = _configuration["Alien:AzureKeyVault:ClientSecret"],
            EncryptionKey = _configuration["Alien:AzureKeyVault:EncryptionKey"],
            options = myProxy
        });
    }

    [Test, Order(1)]
    public async Task TestGetSecret()
    {
        #region Arrange
        #endregion

        #region Act
        string secret = await _secretProvider.GetSecretAsync("MySecret");
        string secret2 = await _secretProvider.GetSecretAsync("EncryptedSecret");
        #endregion

        #region Assert
        Assert.That(secret, Is.EqualTo("I'am an alien"));
        #endregion
    }
}
