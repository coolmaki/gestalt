using Microsoft.EntityFrameworkCore;
using Passport.Core.Domain.Entities;
using Passport.Infrastructure.Data.Configurations.Postgres;

namespace Passport.Infrastructure.Data;

public sealed class PassportDbContext : DbContext
{
    public PassportDbContext(DbContextOptions<PassportDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PasskeyCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new RecoveryCodeConfiguration());
    }
}