namespace Passport.Core.Application.Configuration;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class AccessTokenConfiguration
{
    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    public int LifetimeMinutes { get; set; } = 15;

    /// <summary>
    /// Token issuer (this Passport instance's base URL).
    /// </summary>
    public string Issuer { get; set; } = "https://localhost:5001";

    /// <summary>
    /// Token audience (the Supercluster ecosystem).
    /// </summary>
    public string Audience { get; set; } = "supercluster";
}