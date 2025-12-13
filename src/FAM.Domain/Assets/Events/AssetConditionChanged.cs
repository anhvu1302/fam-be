using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Điều kiện/tình trạng tài sản thay đổi
/// </summary>
public sealed record AssetConditionChanged : IDomainEvent
{
    public long AssetId { get; init; }
    public int? OldConditionId { get; init; }
    public int? NewConditionId { get; init; }
    public string? Reason { get; init; }
    public int? ChangedBy { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
