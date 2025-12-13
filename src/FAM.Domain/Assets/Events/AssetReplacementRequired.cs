using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản cần được thay thế
/// </summary>
public sealed record AssetReplacementRequired : IDomainEvent
{
    public long AssetId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public decimal? ReplacementCost { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
