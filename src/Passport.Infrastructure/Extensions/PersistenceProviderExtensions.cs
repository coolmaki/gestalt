using Passport.Infrastructure.Configuration;

namespace Passport.Infrastructure.Extensions;

internal static class PersistenceProviderExtensions
{
    public static void Configure(this PersistenceProvider provider, Action configureSqlite, Action configurePostgres)
    {
        var configure = provider switch
        {
            PersistenceProvider.Sqlite => configureSqlite,
            PersistenceProvider.Postgres => configurePostgres,
            _ => throw new InvalidOperationException("Invalid PersistenceProvider value."),
        };

        configure.Invoke();
    }
}