namespace Passport.Infrastructure.Configuration;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class InfrastructureConfiguration
{
    public const string SectionName = "Passport:Infrastructure";

    public PersistenceConfiguration Persistence { get; init; } = new();

    public Fido2Config Fido2 { get; init; } = new();
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class Fido2Config
{
    /// <summary>
    /// The relying party domain (RP ID). The browser enforces this against the
    /// origin of the page making the WebAuthn request. In local dev this is
    /// "localhost"; in production it is the app's public domain.
    /// </summary>
    public string ServerDomain { get; init; } = "localhost";

    /// <summary>
    /// The user-visible name of the relying party. Shown in the browser passkey
    /// prompt (e.g., "Sign in with Passport").
    /// </summary>
    public string ServerName { get; init; } = "Passport";

    /// <summary>
    /// The origin of the web page making WebAuthn requests. The browser sends
    /// this and the library validates it against the configured origin.
    /// </summary>
    public string Origin { get; init; } = "http://localhost:5000";
}