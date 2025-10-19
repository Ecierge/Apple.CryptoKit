namespace Apple.CryptoKit;

using Foundation;
using Security;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

public static class Cng
{
    public static RsaSecurityKey Create(string name, CngKeyUsages usages, CngProvider? provider)
    {
        NSData rawKeyData = ACKeychainStorage.IsKeyExist(name, false)
            ? ACKeychainStorage.GetKey(name, false)
            : ACKeychainStorage.CreateKey(name, "rsa", 2048, false, !(usages == CngKeyUsages.Signing), true);

        var keyData = new byte[(int)rawKeyData.Length];
        Marshal.Copy(rawKeyData.Bytes, keyData, 0, keyData.Length);

        RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(keyData, out _);
        return new RsaSecurityKey(rsa);
    }

    public static RsaSecurityKey Open(string name, bool isPublic)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (!ACKeychainStorage.IsKeyExist(name, isPublic))
            throw new InvalidOperationException($"Key '{name}' ({(isPublic ? "public" : "private")}) not found.");

        NSData raw = ACKeychainStorage.GetKey(name, isPublic);
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

        return new RsaSecurityKey(rsa);

        static bool TryImport(Action import)
        {
            try { import(); return true; }
            catch { return false; }
        }
    }

    public static bool Exists(string name, bool isPublic)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        return ACKeychainStorage.IsKeyExist(name, isPublic);
    }

    public static void Delete(string name, bool isPublic)
    {
        throw new NotImplementedException("Delete functionality not available in current ACKeychainStorage implementation");
    }
}