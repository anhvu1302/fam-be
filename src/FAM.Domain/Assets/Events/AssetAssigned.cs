using FAM.Domain.Common;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản được bàn giao
/// </summary>
public sealed record AssetAssigned : IDomainEvent
{
    public required long AssetId { get; init; }
    public string AssigneeType { get; init; } = string.Empty;
    public int AssigneeId { get; init; }
    public long? ByUserId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}