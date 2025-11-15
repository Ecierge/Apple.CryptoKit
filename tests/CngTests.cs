namespace Apple.CryptoKit;

using Foundation;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;


[TestClass]
public class CngKeyTests
{
    private const string CngKeyTestCategory = "CngKey";

    public TestContext TestContext { get; set; }

    private string GetKeyName()
    {
        var name = TestContext.TestName + "_" + Guid.NewGuid().ToString();
        TestContext.Properties["KeyName"] = name;
        return name;
    }

    [TestMethod]
    [TestCategory(CngKeyTestCategory)]
    [DataRow(CngKeyUsages.Signing)]
    [DataRow(CngKeyUsages.Decryption)]
    public void CreateRsaKey_ShouldReturnValidKey(CngKeyUsages usage)
    {
        // Arrange & Act
        var testKeyName = GetKeyName();
        var key = CngKey.Create(CngAlgorithm.Rsa, testKeyName, usage, null);
        var rsaSecurityKey = new RsaSecurityKey(key);

        // Assert
        Assert.IsNotNull(key);
        Assert.IsNotNull(rsaSecurityKey.Rsa);
        Assert.IsGreaterThanOrEqualTo(key.KeySize, 2048);
    }

    [TestMethod]
    [TestCategory(CngKeyTestCategory)]
    [DataRow(CngKeyUsages.Signing)]
    [DataRow(CngKeyUsages.Decryption)]
    public void OpenPrivateKey_ShouldReturnValidKey(CngKeyUsages usage)
    {
        // Arrange
        var testKeyName = GetKeyName();
        // Ensure key exists
        CngKey.Create(CngAlgorithm.Rsa, testKeyName, usage, null);

        // Act
        var key = CngKey.Open(testKeyName, usage);
        var rsaSecurityKey = new RsaSecurityKey(key);

        // Assert
        Assert.IsNotNull(key);
        Assert.IsNotNull(rsaSecurityKey.Rsa);
        Assert.IsGreaterThanOrEqualTo(key.KeySize, 2048);
    }

    [TestMethod]
    [TestCategory(CngKeyTestCategory)]
    [DataRow(CngKeyUsages.Signing)]
    [DataRow(CngKeyUsages.Decryption)]
    public void KeyExists_ShouldReturnTrueForExistingKey(CngKeyUsages usage)
    {
        // Arrange
        var testKeyName = GetKeyName();
        CngKey.Create(CngAlgorithm.Rsa, testKeyName, usage, null);

        // Act
        var exists = CngKey.Exists(testKeyName, usage);

        // Assert
        Assert.IsTrue(exists);
    }

    [TestMethod]
    [TestCategory(CngKeyTestCategory)]
    [DataRow(CngKeyUsages.Signing)]
    [DataRow(CngKeyUsages.Decryption)]
    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public void KeyExists_ShouldReturnFalseForNonExistingKey(CngKeyUsages usage)
    {
        // Arrange
        var nonExistingKey = GetKeyName() + "_NonExisting";

        // Act
        var exists = CngKey.Exists(nonExistingKey, usage);

        // Assert
        Assert.IsFalse(exists);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            var keyName = (string)TestContext.Properties["KeyName"]!;
            CngKey.Delete(keyName, CngKeyUsages.Signing);
            CngKey.Delete(keyName, CngKeyUsages.Decryption);

        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
