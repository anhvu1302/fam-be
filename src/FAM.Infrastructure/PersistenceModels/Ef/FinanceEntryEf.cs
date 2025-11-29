using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for FinanceEntry
/// </summary>
[Table("finance_entries")]
public class FinanceEntryEf : EntityEf
{
    public long AssetId { get; set; }
    public DateTime Period { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? BookValueAfter { get; set; }

    // Navigation properties
    public AssetEf Asset { get; set; } = null!;
    // Note: CreatedBy navigation is inherited from EntityEf base class
}