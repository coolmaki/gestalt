using Microsoft.EntityFrameworkCore;

using Passport.Core.Domain.Entities;
using Passport.Infrastructure.Configuration;

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
        Action<ModelBuilder> configure = _config.Provider switch
        {
            PersistenceProvider.Sqlite => ConfigureSqlite,
            PersistenceProvider.Postgres => ConfigurePostgres,
            _ => throw new InvalidOperationException("Invalid PersistenceProvider value."),
        };

        configure.Invoke(modelBuilder);
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