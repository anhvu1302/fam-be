using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Khấu hao tài sản được tính toán
/// </summary>
public sealed record AssetDepreciationCalculated : IDomainEvent
{
    public long AssetId { get; init; }
    public DateTime Period { get; init; }
    public decimal DepreciationAmount { get; init; }
    public decimal CurrentBookValue { get; init; }
    public decimal AccumulatedDepreciation { get; init; }
    public int? CalculatedBy { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
