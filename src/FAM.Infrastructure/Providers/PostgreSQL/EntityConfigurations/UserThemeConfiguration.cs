using FAM.Domain.Users.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class UserThemeConfiguration : IEntityTypeConfiguration<UserTheme>
{
    public void Configure(EntityTypeBuilder<UserTheme> entity)
    {
        entity.HasKey(ut => ut.Id);

        entity.Property(ut => ut.UserId).IsRequired();
        entity.Property(ut => ut.Theme).IsRequired().HasMaxLength(50);
        entity.Property(ut => ut.PrimaryColor).HasMaxLength(20);
        entity.Property(ut => ut.Transparency).HasPrecision(3, 2); // 0.00 to 1.00
        entity.Property(ut => ut.BorderRadius).IsRequired();
        entity.Property(ut => ut.DarkTheme).HasDefaultValue(false);
        entity.Property(ut => ut.PinNavbar).HasDefaultValue(false);
        entity.Property(ut => ut.CompactMode).HasDefaultValue(false);
        entity.Property(ut => ut.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(ut => !ut.IsDeleted);

        // Index - one theme per user
        entity.HasIndex(ut => ut.UserId)
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_user_themes_user_id");

        // Relationships
        entity.HasOne(ut => ut.User)
            .WithMany()
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
