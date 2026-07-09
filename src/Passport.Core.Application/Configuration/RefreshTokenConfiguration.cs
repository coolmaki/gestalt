namespace Passport.Core.Application.Configuration;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class RefreshTokenConfiguration
{
    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    public int LifetimeDays { get; set; } = 30;

    /// <summary>
    /// When enabled, each refresh revokes the old token and issues a new one.
    /// </summary>
    public bool RotationEnabled { get; set; } = true;
}