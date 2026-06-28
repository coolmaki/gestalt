using Microsoft.EntityFrameworkCore;
using Passport.Core.Domain.Entities;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Extensions;

namespace Passport.Infrastructure.Persistence;

internal sealed class PassportDbContext(DbContextOptions<PassportDbContext> options, PersistenceConfiguration config)
    : DbContext(options)
{
    internal DbSet<User> Users => Set<User>();
    internal DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        config.Provider.Configure(
            configureSqlite: () =>
            {
                modelBuilder.ApplyConfiguration(new Configurations.Sqlite.UserConfiguration());
                modelBuilder.ApplyConfiguration(new Configurations.Sqlite.PasskeyCredentialConfiguration());
                modelBuilder.ApplyConfiguration(new Configurations.Sqlite.RecoveryCodeConfiguration());
            },
            configurePostgres: () =>
            {
                modelBuilder.ApplyConfiguration(new Configurations.Postgres.UserConfiguration());
                modelBuilder.ApplyConfiguration(new Configurations.Postgres.PasskeyCredentialConfiguration());
                modelBuilder.ApplyConfiguration(new Configurations.Postgres.RecoveryCodeConfiguration());
            });
    }
}