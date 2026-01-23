namespace Apple.CryptoKit.Interop;

using Foundation;
using ObjCRuntime;
using Security;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[BaseType(typeof(NSObject))]
public interface ACKeychainStorage
{
    [Static]
    [Export("isKeyExist:isPublic:")]
    public bool IsKeyExist(string keyName, bool isPublic);

    [Static]
    [Export("getKey:isPublic:error:")]
    public NSData GetKey(string keyName, bool isPublic, out NSError error);

    [Static]
    [Export("createRSAKeyAnsStoreInkeychain:algorithm:keySize:isKeyPublic:keyUsage:overwrite:error:")]
    public NSData CreateKey(string keyName, string algorithm, int keySize, bool isPublic, bool keyUsages, bool overwrite, out NSError error);
}
