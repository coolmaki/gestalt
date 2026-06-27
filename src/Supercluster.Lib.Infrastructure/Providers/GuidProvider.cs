using Supercluster.Lib.Application.Providers;

namespace Supercluster.Lib.Infrastructure.Providers;

internal sealed class GuidProvider : IGuidProvider
{
    public Guid NewGuid()
    {
        return Guid.CreateVersion7();
    }
}