#!/bin/bash
set -e

SCHEME="KeychainStorage"
OUTPUT_DIR="./build"
NATIVE_LIB_DIR="../../NativeLib"

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

rm -rf "$NATIVE_LIB_DIR/KeychainStorage-ios.xcframework"
rm -rf "$NATIVE_LIB_DIR/KeychainStorage-catalyst.xcframework"

# iOS (Device)
xcodebuild archive \
  -scheme $SCHEME \
  -destination "generic/platform=iOS" \
  -archivePath "$OUTPUT_DIR/ios_devices.xcarchive" \
  SKIP_INSTALL=NO BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# iOS (Simulator)
xcodebuild archive \
  -scheme $SCHEME \
  -destination "generic/platform=iOS Simulator" \
  -archivePath "$OUTPUT_DIR/ios_simulator.xcarchive" \
  SKIP_INSTALL=NO BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Mac Catalyst
xcodebuild archive \
  -scheme $SCHEME \
  -destination "generic/platform=macOS,variant=Mac Catalyst" \
  -archivePath "$OUTPUT_DIR/maccatalyst.xcarchive" \
  SKIP_INSTALL=NO BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Create xcframeworks
xcodebuild -create-xcframework \
  -framework "$OUTPUT_DIR/ios_devices.xcarchive/Products/Library/Frameworks/$SCHEME.framework" \
  -framework "$OUTPUT_DIR/ios_simulator.xcarchive/Products/Library/Frameworks/$SCHEME.framework" \
  -output "$NATIVE_LIB_DIR/KeychainStorage-ios.xcframework"

xcodebuild -create-xcframework \
  -framework "$OUTPUT_DIR/maccatalyst.xcarchive/Products/Library/Frameworks/$SCHEME.framework" \
  -output "$NATIVE_LIB_DIR/KeychainStorage-catalyst.xcframework"

echo "XCFrameworks built and copied to $NATIVE_LIB_DIR"

# Build NuGet package
dotnet pack ../../Apple.CryptoKit.csproj -c Release

echo "NuGet package built in bin/Release"

