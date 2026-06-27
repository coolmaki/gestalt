using Supercluster.Lib.Application.Providers;

namespace Supercluster.Lib.Infrastructure.Providers;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}
