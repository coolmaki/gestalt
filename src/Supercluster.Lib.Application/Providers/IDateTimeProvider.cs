namespace Supercluster.Lib.Application.Providers;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow();
}