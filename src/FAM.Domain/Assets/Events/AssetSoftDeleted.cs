using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản bị soft delete (xóa mềm)
/// </summary>
public sealed record AssetSoftDeleted : IDomainEvent
{
    public required long AssetId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public long? DeletedBy { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}