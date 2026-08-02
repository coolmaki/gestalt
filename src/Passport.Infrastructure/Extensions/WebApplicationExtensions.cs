using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Passport.Infrastructure.Persistence;

namespace Passport.Infrastructure.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/> that handle
/// infrastructure initialization, such as ensuring the database is
/// created and any pending EF Core migrations are applied on startup.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Ensures the database exists and applies any pending EF Core migrations.
    /// Works for both SQLite (creates the database file automatically) and
    /// Postgres (applies pending migrations to the configured database).
    /// Safe to call on every startup — <see cref="DatabaseFacade.MigrateAsync"/>
    /// only applies migrations that haven't been run yet.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PassportDbContext>();
        await db.Database.MigrateAsync();
    }
}