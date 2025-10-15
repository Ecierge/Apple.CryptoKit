namespace Apple.CryptoKit.Tests;

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Foundation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass(DisplayName = "Cng Tests")]
public class CngTests
{
    private static string NewKeyName(string suffix) => $"com.example.testkey.{suffix}.{Guid.NewGuid():N}";

    private static bool IsApplePlatform() =>
        OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst();

    private static RSA RsaFromNSData(NSData data, bool isPublic)
    {
        var bytes = new byte[(int)data.Length];
        Marshal.Copy(data.Bytes, bytes, 0, bytes.Length);

        var rsa = RSA.Create();
        if (isPublic)
        {
            if (!Try(() => rsa.ImportSubjectPublicKeyInfo(bytes, out _)) &&
                !Try(() => rsa.ImportRSAPublicKey(bytes, out _)))
                throw new CryptographicException("Unsupported public key format.");
        }
        else
        {
            if (!Try(() => rsa.ImportPkcs8PrivateKey(bytes, out _)) &&
                !Try(() => rsa.ImportRSAPrivateKey(bytes, out _)))
                throw new CryptographicException("Unsupported private key format.");
        }
        return rsa;

        static bool Try(Action a) { try { a(); return true; } catch { return false; } }
    }

    private static void AssertSameModulus(RSA expected, RsaSecurityKey actual)
    {
        var e = expected.ExportParameters(true);
        var a = actual.Rsa.ExportParameters(true);
        CollectionAssert.AreEqual(e.Modulus, a.Modulus, "RSA modulus does not match.");
    }

    [TestMethod(DisplayName = "Exists: false → true after CreateKey")]
    public void Exists_False_Then_True_After_Create()
    {
        if (!IsApplePlatform()) { Assert.Inconclusive("This test runs on iOS/MacCatalyst only."); return; }

        var name = NewKeyName("exists");
        try { Cng.Delete(name, isPublic: false); } catch { }

        Assert.IsFalse(Cng.Exists(name, isPublic: false), "Key should not exist before creation.");

        var created = Cng.CreateKey(name, "rsa", 2048, isPublic: false, keyUsages: true, overwrite: true);
        Assert.IsNotNull(created);

        Assert.IsTrue(Cng.Exists(name, isPublic: false), "Key should exist after creation.");

        Cng.Delete(name, isPublic: false);
        Assert.IsFalse(Cng.Exists(name, isPublic: false), "Key should be deleted.");
    }

    [TestMethod(DisplayName = "Open returns existing key and modulus matches")]
    public void Open_Returns_Key_Matching_Keychain()
    {
        if (!IsApplePlatform()) { Assert.Inconclusive("This test runs on iOS/MacCatalyst only."); return; }

        var name = NewKeyName("open");
        try { Cng.Delete(name, false); } catch { }
        var raw = Cng.CreateKey(name, "rsa", 2048, isPublic: false, keyUsages: true, overwrite: true);
        Assert.IsNotNull(raw);

        using var expectedRsa = RsaFromNSData(raw, isPublic: false);

        var opened = Cng.Open(name, isPublic: false);
        Assert.IsNotNull(opened);

        AssertSameModulus(expectedRsa, opened);

        Cng.Delete(name, false);
    }

    [TestMethod(DisplayName = "Create uses existing key when present")]
    public void Create_Uses_Existing_Key()
    {
        if (!IsApplePlatform()) { Assert.Inconclusive("This test runs on iOS/MacCatalyst only."); return; }

        var name = NewKeyName("create-existing");
        try { Cng.Delete(name, false); } catch { }
        var rawExisting = Cng.CreateKey(name, "rsa", 2048, isPublic: false, keyUsages: true, overwrite: true);
        using var expectedRsa = RsaFromNSData(rawExisting, isPublic: false);

        var resultKey = Cng.Create(name, CngKeyUsages.Signing, provider: null);
        Assert.IsNotNull(resultKey);

        AssertSameModulus(expectedRsa, resultKey);

        Cng.Delete(name, false);
    }

    [TestMethod(DisplayName = "Delete removes key from Keychain")]
    public void Delete_Removes_Key()
    {
        if (!IsApplePlatform()) { Assert.Inconclusive("This test runs on iOS/MacCatalyst only."); return; }

        var name = NewKeyName("delete");
        try { Cng.Delete(name, false); } catch { }
        var created = Cng.CreateKey(name, "rsa", 2048, isPublic: false, keyUsages: true, overwrite: true);
        Assert.IsTrue(Cng.Exists(name, false), "Key should exist before deletion.");

        Cng.Delete(name, false);

        Assert.IsFalse(Cng.Exists(name, false), "Key should be deleted.");
    }
}