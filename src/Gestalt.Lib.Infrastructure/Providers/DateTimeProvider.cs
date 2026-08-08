using Gestalt.Lib.Application.Providers;

namespace Gestalt.Lib.Infrastructure.Providers;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}
