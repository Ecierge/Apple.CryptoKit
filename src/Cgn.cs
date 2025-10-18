namespace Apple.CryptoKit;

using Foundation;
using Security;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;
using static ACKeychainStorage;

public interface Cng
{
    internal NSData CreateKey(string keyName, string algorithm, int keySize, bool isPublic, bool keyUsages, bool overwrite);

    public RsaSecurityKey Create(string name, CngKeyUsages usages, CngProvider? provider)
    {
        NSData rawKeyData = IsKeyExist(name, false)
            ? GetKey(name, false)
            : CreateKey(name, "rsa", 2048, false, !(usages == CngKeyUsages.Signing), true);

        var keyData = new byte[(int)rawKeyData.Length];
        Marshal.Copy(rawKeyData.Bytes, keyData, 0, keyData.Length);

        RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(keyData, out _);
        return new RsaSecurityKey(rsa);
    }

    public RsaSecurityKey Open(string name, bool isPublic)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (!IsKeyExist(name, isPublic))
            throw new InvalidOperationException($"Key '{name}' ({(isPublic ? "public" : "private")}) not found.");

        NSData raw = GetKey(name, isPublic);
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

    public bool Exists(string name, bool isPublic)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var query = new SecRecord(isPublic ? SecKind.PublicKey : SecKind.PrivateKey)
        {
            Label = name,
            MatchLimit = SecMatchLimit.One,
            ReturnRef = false
        };

        var status = SecKeyChain.QueryAsRecord(query, out _);
        return status == SecStatusCode.Success;
    }


    public static void Delete(string name, bool isPublic)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rec = new SecRecord(isPublic ? SecKind.PublicKey : SecKind.PrivateKey)
        {
            Label = name
        };

        var status = SecKeyChain.Remove(rec);
        if (status == SecStatusCode.ItemNotFound) return;
        if (status != SecStatusCode.Success)
            throw new CryptographicException($"Keychain delete failed: {status}");
    }
}
