using FAM.Domain.Common;

namespace FAM.Domain.Finance;

/// <summary>
/// Bút toán tài chính (khấu hao, điều chỉnh, xóa sổ)
/// </summary>
public class FinanceEntry : Entity
{
    public long AssetId { get; private set; }
    public DateTime Period { get; private set; }
    public string EntryType { get; private set; } = string.Empty; // depreciation, adjustment, writeoff
    public decimal Amount { get; private set; }
    public decimal? BookValueAfter { get; private set; }

    // Navigation properties
    public Assets.Asset Asset { get; set; } = null!;
    public Users.User? Creator { get; set; }

    private FinanceEntry() { }

    public static FinanceEntry Create(
        long assetId,
        DateTime period,
        string entryType,
        decimal amount,
        decimal? bookValueAfter = null,
        long? createdById = null)
    {
        return new FinanceEntry
        {
            AssetId = assetId,
            Period = period,
            EntryType = entryType,
            Amount = amount,
            BookValueAfter = bookValueAfter,
            CreatedAt = DateTime.UtcNow,
            CreatedById = createdById
        };
    }
}
