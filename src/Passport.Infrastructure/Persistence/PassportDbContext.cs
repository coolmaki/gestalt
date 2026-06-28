using Microsoft.EntityFrameworkCore;

using Passport.Core.Domain.Entities;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Extensions;

namespace Passport.Infrastructure.Persistence;

internal sealed class PassportDbContext(
    DbContextOptions<PassportDbContext> options,
    InfrastructureConfiguration config)
    : DbContext(options)
{
    private readonly PersistenceConfiguration _config = config.Persistence;

    internal DbSet<User> Users => Set<User>();
    internal DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _config.Provider.Configure(
            configureSqlite: () => ConfigureSqlite(modelBuilder),
            configurePostgres: () => ConfigurePostgres(modelBuilder));
    }

    private static void ConfigureSqlite(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.Sqlite.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.Sqlite.PasskeyCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.Sqlite.RecoveryCodeConfiguration());
    }

    private static void ConfigurePostgres(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.Postgres.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.Postgres.PasskeyCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.Postgres.RecoveryCodeConfiguration());
    }
}