using Microsoft.EntityFrameworkCore;
using Passport.Core.Domain.Entities;
using Passport.Infrastructure.Persistence.Configurations.Postgres;

namespace Passport.Infrastructure.Persistence;

internal sealed class PassportDbContext(DbContextOptions<PassportDbContext> options, PassportInfrastructureConfiguration config)
    : DbContext(options)
{
    private readonly string _provider = config.Provider;

    internal DbSet<User> Users => Set<User>();
    internal DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (string.Equals(_provider, "Postgres", StringComparison.OrdinalIgnoreCase))
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PasskeyCredentialConfiguration());
            modelBuilder.ApplyConfiguration(new RecoveryCodeConfiguration());
        }
        else
        {
            modelBuilder.ApplyConfiguration(new Configurations.Sqlite.UserConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.Sqlite.PasskeyCredentialConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.Sqlite.RecoveryCodeConfiguration());
        }
    }
}