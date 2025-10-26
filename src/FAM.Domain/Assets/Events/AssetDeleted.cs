using FAM.Domain.Common;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản bị xóa/xóa sổ
/// </summary>
public sealed record AssetDeleted : IDomainEvent
{
    public long AssetId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
