using DotNetCoreSqlDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCoreSqlDb.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.EmailNormalized)
            .HasDatabaseName("ux_users_email_normalized")
            .IsUnique();

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.EmailNormalized)
            .HasColumnName("email_normalized")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(150);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.EmailVerifiedAt)
            .HasColumnName("email_verified_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
