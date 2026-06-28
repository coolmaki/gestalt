using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Infrastructure.Persistence.Configurations.Sqlite;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Shadow PK since User has no domain ID
        builder.Property<Guid>("Id")
            .ValueGeneratedOnAdd();

        builder.HasKey("Id");

        // Email value object → string column
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

        // Owned collection of passkeys
        builder.OwnsMany(u => u.Passkeys, pb =>
        {
            pb.WithOwner().HasForeignKey("UserId");

            pb.Property<Guid>("Id")
                .ValueGeneratedOnAdd();

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

            // Ignore PublicKey length validation at the DB level — it's validated in domain
        });

        // Ignore domain events
        builder.Ignore(u => u.Events);
    }
}