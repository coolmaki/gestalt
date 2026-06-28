namespace Passport.Core.Application.Configuration;

public sealed class ApplicationConfiguration
{
    public const string SectionName = "Passport:Application";

    /// <summary>
    /// Base URL for the Passport service (used in email links, issuer claims, etc.).
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:5001";
}