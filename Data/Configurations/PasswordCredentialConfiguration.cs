using DotNetCoreSqlDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCoreSqlDb.Data.Configurations;

public sealed class PasswordCredentialConfiguration : IEntityTypeConfiguration<PasswordCredential>
{
    public void Configure(EntityTypeBuilder<PasswordCredential> builder)
    {
        builder.ToTable("password_credentials");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.PasswordChangedAt)
            .HasColumnName("password_changed_at")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithOne(x => x.PasswordCredential)
            .HasForeignKey<PasswordCredential>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_password_credentials_user");
    }
}
