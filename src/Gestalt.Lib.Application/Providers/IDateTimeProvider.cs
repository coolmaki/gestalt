namespace Gestalt.Lib.Application.Providers;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow();
}