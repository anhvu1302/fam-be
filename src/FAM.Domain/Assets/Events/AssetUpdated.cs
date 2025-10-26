using FAM.Domain.Common;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản được cập nhật
/// </summary>
public sealed record AssetUpdated : IDomainEvent
{
    public long AssetId { get; init; }
    public string? PropertyChanged { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
