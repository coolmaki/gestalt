using Microsoft.EntityFrameworkCore;
using Passport.Core.Domain.Entities;
using Passport.Infrastructure.Persistence.Configurations.Postgres;

namespace Passport.Infrastructure.Persistence;

public sealed class PassportDbContext : DbContext
{
    private readonly string _provider;

    public PassportDbContext(DbContextOptions<PassportDbContext> options, PassportInfrastructureConfiguration config)
        : base(options)
    {
        _provider = config.Provider;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();

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