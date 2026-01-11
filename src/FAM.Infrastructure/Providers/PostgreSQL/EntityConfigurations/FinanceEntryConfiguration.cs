using FAM.Domain.Finance;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class FinanceEntryConfiguration : IEntityTypeConfiguration<FinanceEntry>
{
    public void Configure(EntityTypeBuilder<FinanceEntry> entity)
    {
        entity.HasKey(fe => fe.Id);
        entity.Property(fe => fe.EntryType).IsRequired().HasMaxLength(50);
        entity.Property(fe => fe.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(fe => !fe.IsDeleted);

        // Indexes
        entity.HasIndex(fe => fe.AssetId).HasDatabaseName("ix_finance_entries_asset_id");
        entity.HasIndex(fe => fe.Period).HasDatabaseName("ix_finance_entries_period");
        entity.HasIndex(fe => fe.EntryType).HasDatabaseName("ix_finance_entries_entry_type");

        // Relationships
        entity.HasOne(fe => fe.Asset).WithMany(a => a.FinanceEntries).HasForeignKey(fe => fe.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
