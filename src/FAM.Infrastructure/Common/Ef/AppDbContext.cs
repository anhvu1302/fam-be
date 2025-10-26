using FAM.Domain.Companies;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Common.Ef;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.TaxCode).HasMaxLength(50);
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.Property(c => c.IsDeleted).HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(c => !c.IsDeleted);
        });
    }
}