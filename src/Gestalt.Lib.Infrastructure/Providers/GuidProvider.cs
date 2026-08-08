using Gestalt.Lib.Application.Providers;

namespace Gestalt.Lib.Infrastructure.Providers;

internal sealed class GuidProvider : IGuidProvider
{
    public Guid NewGuid()
    {
        return Guid.CreateVersion7();
    }
}