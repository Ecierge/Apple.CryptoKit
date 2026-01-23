namespace Apple.CryptoKit;

using Foundation;
using Security;
using System;
using Xunit;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;


//[TestFixture]
public class CngKeyTests : IDisposable
{
    private const string CngKeyTestCategory = "CngKey";

    private string? _keyName;

    private static bool IsMacCatalyst()
    {
#if MACCATALYST
        return true;
#else
        return false;
#endif
    }

    private string GetKeyName()
    {
        _keyName = "CngKey_" + Guid.NewGuid().ToString();
        return _keyName;
    }

    [Theory]
    [Trait("Category", CngKeyTestCategory)]
    [InlineData(CngKeyUsages.Signing)]
    [InlineData(CngKeyUsages.Decryption)]
    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public void CreateRsaKey_ShouldReturnValidKey(CngKeyUsages usage)
    {
        //if (IsMacCatalyst())
        //{
            // Skip: SecKeyCopyExternalRepresentation not supported on MacCatalyst
            //return;
        //}

        // Arrange & Act
        var testKeyName = GetKeyName();
        RSA key = CngKey.Create(CngAlgorithm.Rsa, testKeyName, usage, null);
        var rsaSecurityKey = new RsaSecurityKey(key);

        // Assert
        Assert.NotNull(key);
        Assert.NotNull(rsaSecurityKey.Rsa);
        Assert.True(key.KeySize >= 2048);
    }

    [Theory]
    [Trait("Category", CngKeyTestCategory)]
    [InlineData(CngKeyUsages.Signing)]
    [InlineData(CngKeyUsages.Decryption)]
    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public void OpenPrivateKey_ShouldReturnValidKey(CngKeyUsages usage)
    {
        //if (IsMacCatalyst())
        //{
            // Skip: SecKeyCopyExternalRepresentation not supported on MacCatalyst
        //    return;
        //}

        // Arrange
        var testKeyName = GetKeyName();
        // Ensure key exists
        CngKey.Create(CngAlgorithm.Rsa, testKeyName, usage, null);

        // Act
        var key = CngKey.Open(testKeyName, usage);
        var rsaSecurityKey = new RsaSecurityKey(key);

        // Assert
        Assert.NotNull(key);
        Assert.NotNull(rsaSecurityKey.Rsa);
        Assert.True(key.KeySize >= 2048);
    }

    [Theory]
    [Trait("Category", CngKeyTestCategory)]
    [InlineData(CngKeyUsages.Signing)]
    [InlineData(CngKeyUsages.Decryption)]
    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public void KeyExists_ShouldReturnTrueForExistingKey(CngKeyUsages usage)
    {
        //if (IsMacCatalyst())
        //{
        //    // Skip: SecKeyCopyExternalRepresentation not supported on MacCatalyst
        //    return;
        //}
        
        // Arrange
        var testKeyName = GetKeyName();
        CngKey.Create(CngAlgorithm.Rsa, testKeyName, usage, null);

        // Act
        var exists = CngKey.Exists(testKeyName, usage);

        // Assert
        Assert.True(exists);
    }

    [Theory]
    [Trait("Category", CngKeyTestCategory)]
    [InlineData(CngKeyUsages.Signing)]
    [InlineData(CngKeyUsages.Decryption)]
    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    //[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    //[System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public void KeyExists_ShouldReturnFalseForNonExistingKey(CngKeyUsages usage)
    {
        // Arrange
        var nonExistingKey = GetKeyName() + "_NonExisting";

        // Act
        var exists = CngKey.Exists(nonExistingKey, usage);

        // Assert
        Assert.False(exists);
    }

    //[TearDown]
    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public void Dispose()
    {
        try
        {
            if (_keyName != null && CngKey.Exists(_keyName, CngKeyUsages.Signing))
            {
                CngKey.Delete(_keyName, CngKeyUsages.Signing);
                CngKey.Delete(_keyName, CngKeyUsages.Decryption);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
