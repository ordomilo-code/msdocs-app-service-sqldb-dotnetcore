using DotNetCoreSqlDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCoreSqlDb.Data.Configurations;

public sealed class AuthIdentityConfiguration : IEntityTypeConfiguration<AuthIdentity>
{
    public void Configure(EntityTypeBuilder<AuthIdentity> builder)
    {
        builder.ToTable("auth_identities");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.Provider, x.ProviderSubject })
            .HasDatabaseName("ux_auth_identities_provider_subject")
            .IsUnique();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_auth_identities_user_id");

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProviderSubject)
            .HasColumnName("provider_subject")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.LinkedAt)
            .HasColumnName("linked_at")
            .IsRequired();

        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at");

        builder.HasOne(x => x.User)
            .WithMany(x => x.AuthIdentities)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_auth_identities_user");
    }
}
