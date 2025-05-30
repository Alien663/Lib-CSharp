﻿using NUnit.Framework;
using System;
using Alien.Common.Config;

namespace TestMyLib;

[TestFixture]
public class ConfigTest
{
    [Test, Order(1)]
    public void ReadAppconfig()
    {
        #region Arrange
        AppConfig _config = new AppConfig("appsettings.dev.json");
        #endregion

        #region Action
        #endregion

        #region Assert
        Assert.That(_config.Configuration["ConnectionStrings:DefaultConnection"], Is.EqualTo("This is dev"));
        #endregion
    }
    
    [Test, Order(2)]
    public void AESEncryp()
    {
        #region Arrange
        using AESCrypto crypto = new AESCrypto();
        #endregion

        #region Action
        string encrypted = crypto.Encrypt("This is a test");
        Console.WriteLine(encrypted);
        #endregion

        #region Assert
        Assert.That(encrypted, Is.EqualTo("OGI4NWEzMWEwODR01MCWogPWCwete8VJBxkJU0ZoC3R77TTD7TtzwaKo"));
        #endregion
    }

    [Test, Order(3)]
    public void AESDecrypt()
    {
        #region Arrange
        using AESCrypto crypto = new AESCrypto();
        #endregion

        #region Action
        string decrypted = crypto.Decrypt("OGI4NWEzMWEwODR01MCWogPWCwete8VJBxkJU0ZoC3R77TTD7TtzwaKo");
        #endregion

        #region Assert
        Assert.That(decrypted == "This is a test");
        #endregion
    }
}
