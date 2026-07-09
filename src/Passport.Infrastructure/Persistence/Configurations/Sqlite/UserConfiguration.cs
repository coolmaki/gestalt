using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;
using Passport.Infrastructure.Persistence.Generators;

namespace Passport.Infrastructure.Persistence.Configurations.Sqlite;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property<Guid>("Id")
            .ValueGeneratedOnAdd()
            .HasValueGenerator<ShadowIdGenerator>();

        builder.HasKey("Id");

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, s => Email.Create(s).Value)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.EmailVerified)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.OwnsMany(u => u.Passkeys, pb =>
        {
            pb.WithOwner().HasForeignKey("UserId");

            pb.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasValueGenerator<ShadowIdGenerator>();

            pb.HasKey("Id");

            pb.HasIndex(p => p.CredentialId)
                .IsUnique();

            pb.Property(p => p.DeviceName)
                .HasConversion(d => d != null ? d.Value : null, s => s != null ? DeviceName.Create(s).Value : null)
                .HasMaxLength(100);

            pb.Property(p => p.SignCount)
                .IsRequired();

            pb.Property(p => p.CreatedAt)
                .IsRequired();
        });

        builder.OwnsMany(u => u.RefreshTokens, rb =>
        {
            rb.WithOwner().HasForeignKey("UserId");

            rb.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasValueGenerator<ShadowIdGenerator>();

            rb.HasKey("Id");

            rb.Property("TokenHash")
                .HasMaxLength(64)
                .IsRequired();

            rb.HasIndex("TokenHash")
                .IsUnique();

            rb.Property("ClientId")
                .HasMaxLength(200);

            rb.Property("ExpiresAt")
                .IsRequired();

            rb.Property("IssuedAt")
                .IsRequired();

            rb.Property("RevokedAt");
        });

        builder.Ignore(u => u.Events);
    }
}