using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Infrastructure.Data.Configurations.Postgres;

internal sealed class PasskeyCredentialConfiguration : IEntityTypeConfiguration<PasskeyCredential>
{
    public void Configure(EntityTypeBuilder<PasskeyCredential> builder)
    {
        builder.Property<Guid>("Id")
            .ValueGeneratedOnAdd();

        builder.HasKey("Id");

        builder.HasIndex(p => p.CredentialId)
            .IsUnique();

        builder.Property(p => p.DeviceName)
            .HasConversion(d => d != null ? d.Value : null, s => s != null ? DeviceName.Create(s).Value : null)
            .HasMaxLength(100);

        builder.Property(p => p.SignCount)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}