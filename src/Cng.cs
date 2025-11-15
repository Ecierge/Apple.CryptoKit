namespace Apple.CryptoKit;

using Foundation;
using Security;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Apple.CryptoKit.Interop;

public static class CngKey
{
    private static bool IsPublicKey(CngKeyUsages usages)
     => usages switch
        {
            CngKeyUsages.Decryption => true,
            CngKeyUsages.Signing => false,
            _ => throw new ArgumentOutOfRangeException(nameof(usages), "Unsupported key usage.")
        };

    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    static RSA FromNSData(bool isPublic, NSData raw)
    {
        var bytes = new byte[(int)raw.Length];
        Marshal.Copy(raw.Bytes, bytes, 0, bytes.Length);

        RSA rsa = RSA.Create();

        if (isPublic)
        {
            if (!TryImport(() => rsa.ImportSubjectPublicKeyInfo(bytes, out _)) &&
                !TryImport(() => rsa.ImportRSAPublicKey(bytes, out _)))
            {
                throw new CryptographicException("Unsupported public key format.");
            }
        }
        else
        {
            if (!TryImport(() => rsa.ImportPkcs8PrivateKey(bytes, out _)) &&
                !TryImport(() => rsa.ImportRSAPrivateKey(bytes, out _)))
            {
                throw new CryptographicException("Unsupported private key format.");
            }
        }

        return rsa;

        static bool TryImport(Action import)
        {
            try { import(); return true; }
            catch { return false; }
        }
    }


    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public static RSA Create(CngAlgorithm algorithm, string name, CngKeyUsages usage, CngProvider? provider)
    {
        if (algorithm != CngAlgorithm.Rsa)
            throw new NotSupportedException($"Algorithm '{algorithm.Algorithm}' is not supported. Only RSA is supported.");

        bool isPublic = IsPublicKey(usage);
        string algorithmName = algorithm.Algorithm.ToLower();
        NSData rawKeyData = ACKeychainStorage.CreateKey(name, algorithmName, 2048, false, isPublic, true);
        return FromNSData(isPublic, rawKeyData);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public static RSA Open(string name, CngKeyUsages usage)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        bool isPublic = IsPublicKey(usage);

        NSData raw;
        try
        {
            raw = ACKeychainStorage.GetKey(name, isPublic);
        }
        // TODO: Refine exception handling based on ACKeychainStorage implementation
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Key '{name}' for ({usage}) not found.", ex);
        }
        return FromNSData(isPublic, raw);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("ios12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("maccatalyst12.2")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos12.0")]
    [System.Runtime.Versioning.SupportedOSPlatform("tvos12.2")]
    public static bool Exists(string name, CngKeyUsages usage)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        bool isPublic = IsPublicKey(usage);
        return ACKeychainStorage.IsKeyExist(name, isPublic);
    }

    public static void Delete(string name, CngKeyUsages usage)
    {
        throw new NotImplementedException("Delete functionality not available in current ACKeychainStorage implementation");
    }
}
