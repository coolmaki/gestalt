namespace Supercluster.Lib.Presentation.Http;

/// <summary>
/// API version identifier. Each version maps to a URL path segment (e.g., V1 → "v1").
/// <see cref="None"/> is used for unversioned root-level endpoints (e.g., <c>/.well-known/</c>).
/// Append new values as new API versions are introduced. Older versions are maintained
/// alongside newer ones to avoid breaking existing consumers.
/// </summary>
public enum EndpointVersion
{
    None = 0,
    V1 = 1,
    V2 = 2,
    V3 = 3,
}