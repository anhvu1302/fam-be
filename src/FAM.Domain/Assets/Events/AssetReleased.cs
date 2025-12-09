using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản được thu hồi
/// </summary>
public sealed record AssetReleased : IDomainEvent
{
    public required long AssetId { get; init; }
    public int AssignmentId { get; init; }
    public long? ByUserId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}