using System.ComponentModel.DataAnnotations.Schema;
using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for FinanceEntry
/// </summary>
[Table("finance_entries")]
public class FinanceEntryEf : BaseEntityEf
{
    public long AssetId { get; set; }
    public DateTime Period { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public decimal? BookValueAfter { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }


    // Navigation properties
    public AssetEf Asset { get; set; } = null!;
    // Note: CreatedBy navigation is inherited from EntityEf base class
}