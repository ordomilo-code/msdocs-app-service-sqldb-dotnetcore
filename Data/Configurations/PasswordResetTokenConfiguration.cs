using DotNetCoreSqlDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCoreSqlDb.Data.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.TokenHash)
            .HasDatabaseName("ux_password_reset_tokens_token_hash")
            .IsUnique();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_password_reset_tokens_user_id");

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

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.ConsumedAt)
            .HasColumnName("consumed_at");

        builder.HasOne(x => x.User)
            .WithMany(x => x.PasswordResetTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_password_reset_tokens_user");
    }
}
