using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Infrastructure.Data.Configurations.Postgres;

internal sealed class RecoveryCodeConfiguration : IEntityTypeConfiguration<RecoveryCode>
{
    public void Configure(EntityTypeBuilder<RecoveryCode> builder)
    {
        // RecoveryCodeId value object → Guid
        builder.Property(rc => rc.Id)
            .HasConversion(id => id.Value, g => new RecoveryCodeId(g));

        builder.HasKey(rc => rc.Id);

        // Email value object → string
        builder.Property(rc => rc.Email)
            .HasConversion(e => e.Value, s => Email.Create(s).Value)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(rc => new { rc.Email, rc.Purpose });

        builder.Property(rc => rc.CodeHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(rc => rc.Purpose)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(rc => rc.ExpiresAt)
            .IsRequired();

        builder.Property(rc => rc.CreatedAt)
            .IsRequired();

        builder.Property(rc => rc.UsedAt);
    }
}