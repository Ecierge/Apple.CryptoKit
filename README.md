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

## Developing

Requirements:

- macOS with [Xcode](https://developer.apple.com/xcode/), because the library and its tests target iOS and Mac Catalyst
- the [.NET SDK](https://dotnet.microsoft.com/download) that `global.json` pins
- the `ios` and `maccatalyst` .NET workloads of that SDK. The SDK needs write access to its own folder to install them, so
  use `sudo dotnet workload install ios maccatalyst` when the SDK is installed system-wide. A new SDK feature band has none
  of them installed, so install them again after an SDK update that changes the band (`10.0.4xx` to `10.0.5xx`, for example)

Build with the [FAKE](https://fake.build) script in `build/`:

```shell
./build.sh <optional target>   # build.cmd on Windows
```

Without a target the script packs the solution. The targets are declared in `build/build.fs`, among them `DotnetBuild`,
`DotnetTest`, `DotnetPack`, `FormatCode`, `CheckFormatCode`, `BuildDocs` and `Release <version>`. Any build restores the
local tools in `.config/dotnet-tools.json` first.

Environment variables:

- `CONFIGURATION` sets the configuration of the dotnet commands. It is `Debug`, and `Release` when the `PublishToNuGet` target runs.
- `ENABLE_COVERAGE=1` collects code coverage in the `DotnetTest` target.

The instructions for AI coding assistants are in [`.github/copilot-instructions.md`](.github/copilot-instructions.md), which
[`AGENTS.md`](AGENTS.md) and [`CLAUDE.md`](CLAUDE.md) point to.
