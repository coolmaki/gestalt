namespace Gestalt.Lib.Presentation.Http.Extensions;

public static class EndpointVersionExtensions
{
    /// <summary>
    /// Converts the version to its URL path segment (e.g., <see cref="EndpointVersion.V1"/> → "v1").
    /// </summary>
    public static string ToPathSegment(this EndpointVersion version) => version switch
    {
        EndpointVersion.None => string.Empty,
        EndpointVersion.V1 => "v1",
        EndpointVersion.V2 => "v2",
        EndpointVersion.V3 => "v3",
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported endpoint version."),
    };
}