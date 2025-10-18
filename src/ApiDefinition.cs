namespace Apple.CryptoKit;

using Foundation;
using Security;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[BaseType(typeof(NSObject))]
internal interface ACKeychainStorage
{
    [Static]
    [Export("isKeyExist:isPublic:")]
    internal bool IsKeyExist(string keyName, bool isPublic);

    [Static]
    [Export("getKey:isPublic:")]
    internal NSData GetKey(string keyName, bool isPublic);

    [Static]
    [Export("createRSAKeyAnsStoreInkeychain:algorithm:keySize:isKeyPublic:keyUsage:overwrite:")]
    internal NSData CreateKey(string keyName, string algorithm, int keySize, bool isPublic, bool keyUsages, bool overwrite);
}
