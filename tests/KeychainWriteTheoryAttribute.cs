namespace Apple.CryptoKit;

using Xunit;

/// <summary>
/// A theory that writes to the Keychain. A Mac Catalyst app can do that only when a provisioning profile signs it, which a
/// CI runner does not have: the Keychain answers errSecMissingEntitlement (-34018). Building the tests with
/// <c>SKIP_KEYCHAIN_WRITE_TESTS=true</c> skips these theories on Mac Catalyst. The iOS simulator always runs them.
/// </summary>
public sealed class KeychainWriteTheoryAttribute : TheoryAttribute
{
    public KeychainWriteTheoryAttribute()
    {
#if SKIP_KEYCHAIN_WRITE_TESTS
        Skip = "The Keychain refuses the writes of a Mac Catalyst app that no provisioning profile signs (-34018)";
#endif
    }
}
