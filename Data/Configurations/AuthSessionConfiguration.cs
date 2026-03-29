using DotNetCoreSqlDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCoreSqlDb.Data.Configurations;

public sealed class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSession>
{
    public void Configure(EntityTypeBuilder<AuthSession> builder)
    {
        builder.ToTable("auth_sessions");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.TokenHash)
            .HasDatabaseName("ux_auth_sessions_token_hash")
            .IsUnique();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_auth_sessions_user_id");

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(64)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.LastSeenAt)
            .HasColumnName("last_seen_at");

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at");

        builder.HasOne(x => x.User)
            .WithMany(x => x.AuthSessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_auth_sessions_user");
    }
}
