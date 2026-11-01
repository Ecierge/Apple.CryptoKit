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
- the `ios` and `maccatalyst` .NET workloads. The SDK needs write access to its own folder to install them, so use
  `sudo dotnet workload restore src/Apple.CryptoKit.csproj` when the SDK is installed system-wide. A new SDK feature band has no
  workloads installed, so restore them again after an SDK update that changes the band (`10.0.4xx` to `10.0.5xx`, for example)

.NET for iOS and Mac Catalyst accepts only the Xcode whose major and minor version it was built for, and the workloads are
published some time after Xcode. `Directory.Build.props` therefore turns the check off (`ValidateXcodeVersion`), so the
workloads of Xcode 27.1 build with a newer Xcode. Pass `-p:ValidateXcodeVersion=true` to bring the check back.

Build with the [FAKE](https://fake.build) script in `build/`:

```shell
./build.sh <optional target>   # build.cmd on Windows
```

Without a target the script packs the solution. The targets are declared in `build/build.fs`, among them `DotnetBuild`,
`DotnetTest`, `DotnetPack`, `FormatCode`, `CheckFormatCode`, `BuildDocs` and `Release <version>`. Any build restores the
local tools in `.config/dotnet-tools.json` first.

`DotnetTest` runs the test apps with [XHarness](https://github.com/dotnet/xharness), because `dotnet test` cannot start an
app. It installs each app on its target and writes the results into `TestResults/<target>`. By default it runs the iOS
simulator. Mac Catalyst is opt-in, because its Keychain tests need an app that is signed with a provisioning profile and
otherwise fail with `-34018`.

### Signing the Mac Catalyst test app

The Keychain accepts the writes of a Mac Catalyst app only when the app is signed with a provisioning profile. With the ad-hoc
signing of the repository 6 of the 8 tests fail with `-34018`. To run all of them:

1. In the Apple Developer account create an explicit App ID, a certificate of the type Apple Development and a profile of the
   type macOS App Development for that App ID. The profile lists the Macs it works on, so add this Mac to the devices.
2. Install the certificate in the login keychain and open the downloaded profile.
3. Create `tests/Signing.local.props`, which Git ignores:

   ```xml
   <Project>
     <PropertyGroup>
       <ApplicationId>com.example.Apple.CryptoKit.Tests</ApplicationId>
     </PropertyGroup>
     <PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst'">
       <CodesignKey>Apple Development: Your Name (TEAMMEMBERID)</CodesignKey>
       <CodesignProvision>Name of the profile</CodesignProvision>
     </PropertyGroup>
   </Project>
   ```

4. Run `XHARNESS_TARGETS=maccatalyst ./build.sh DotnetTest`.

A profile that is bound to devices cannot work on the virtual machines of GitHub Actions, so the workflow runs the iOS simulator only.

Environment variables:

- `CONFIGURATION` sets the configuration of the dotnet commands. It is `Debug`, and `Release` when the `PublishToNuGet` target runs.
- `XHARNESS_TARGETS` lists the XHarness targets that `DotnetTest` runs, separated by commas. The default is `ios-simulator-64`;
  `ios-simulator-64,maccatalyst` adds Mac Catalyst.

The instructions for AI coding assistants are in [`.github/copilot-instructions.md`](.github/copilot-instructions.md), which
[`AGENTS.md`](AGENTS.md) and [`CLAUDE.md`](CLAUDE.md) point to.
