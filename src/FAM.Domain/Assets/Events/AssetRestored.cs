using FAM.Domain.Common;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản được khôi phục
/// </summary>
public sealed record AssetRestored : IDomainEvent
{
    public required long AssetId { get; init; }
    public long? RestoredBy { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
