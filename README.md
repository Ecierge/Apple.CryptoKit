# Apple.CryptoKit

.NET bindings for [Apple.CryptoKit](https://developer.apple.com/documentation/cryptokit) framework to use RSA keys in [Uno Platform](https://platform.uno) apps targeting **iOS** and **Mac Catalyst**.  
This package provides a .NET facade (`ACKeychainStorage`) over the native CryptoKit/Keychain APIs.

## Installation

Add GitHub Packages feed to your `nuget.config`:

```xml
<configuration>
  <packageSources>
    <add key="Ecierge GitHub" value="https://nuget.pkg.github.com/Ecierge/index.json" />
  </packageSources>
</configuration>
```