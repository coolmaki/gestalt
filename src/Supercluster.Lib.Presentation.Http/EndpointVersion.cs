namespace Supercluster.Lib.Presentation.Http;

/// <summary>
/// API version identifier. Each version maps to a URL path segment (e.g., V1 → "v1").
/// Append new values as new API versions are introduced. Older versions are maintained
/// alongside newer ones to avoid breaking existing consumers.
/// </summary>
public enum EndpointVersion
{
    V1,
    V2,
    V3,
}