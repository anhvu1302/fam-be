using FAM.Domain.Assets;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users;

namespace FAM.Domain.Finance;

/// <summary>
/// Bút toán tài chính (khấu hao, điều chỉnh, xóa sổ)
/// </summary>
public class FinanceEntry : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter
{
    // Domain fields
    public long AssetId { get; private set; }
    public DateTime Period { get; private set; }
    public string EntryType { get; private set; } = string.Empty; // depreciation, adjustment, writeoff
    public decimal Amount { get; private set; }
    public decimal? BookValueAfter { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
    public User? DeletedBy { get; set; }
    public Asset Asset { get; set; } = null!;
    public User? Creator { get; set; }

    private FinanceEntry()
    {
    }

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

    public void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = deletedById;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdatedAt = DateTime.UtcNow;
    }
}