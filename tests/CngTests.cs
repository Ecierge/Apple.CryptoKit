namespace Apple.CryptoKit;

using Foundation;
using Security;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;


[TestClass]
public class CngTests
{
    private const string TestKeyName = "TestRSAKey";

    [TestMethod]
    [TestCategory("Cng")]
    public void CreateRsaKey_ShouldReturnValidKey()
    {
        // Arrange & Act
        var key = Cng.Create(TestKeyName, CngKeyUsages.Signing, null);

        // Assert
        Assert.IsNotNull(key);
        Assert.IsNotNull(key.Rsa);
        Assert.IsTrue(key.KeySize >= 2048);
    }

    [TestMethod]
    [TestCategory("Cng")]
    public void OpenPrivateKey_ShouldReturnValidKey()
    {
        // Arrange
        // Ensure key exists
        Cng.Create(TestKeyName, CngKeyUsages.Signing, null);

        // Act
        var key = Cng.Open(TestKeyName, false);

        // Assert
        Assert.IsNotNull(key);
        Assert.IsNotNull(key.Rsa);
        Assert.IsTrue(key.KeySize >= 2048);
    }

    [TestMethod]
    [TestCategory("Cng")]
    public void KeyExists_ShouldReturnTrueForExistingKey()
    {
        // Arrange
        Cng.Create(TestKeyName, CngKeyUsages.Signing, null);

        // Act
        var exists = Cng.Exists(TestKeyName, false);

        // Assert
        Assert.IsTrue(exists);
    }

    [TestMethod]
    [TestCategory("Cng")]
    public void KeyExists_ShouldReturnFalseForNonExistingKey()
    {
        // Arrange
        var nonExistingKey = "NonExistingKey123";

        // Act
        var exists = Cng.Exists(nonExistingKey, false);

        // Assert
        Assert.IsFalse(exists);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            // Clean up test keys
            if (Cng.Exists(TestKeyName, false))
            {
                // Note: Delete method is not implemented yet
                // Cng.Delete(TestKeyName, false);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}