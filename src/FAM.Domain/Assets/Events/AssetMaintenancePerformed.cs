using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Bảo trì tài sản được thực hiện
/// </summary>
public sealed record AssetMaintenancePerformed : IDomainEvent
{
    public long AssetId { get; init; }
    public DateTime MaintenanceDate { get; init; }
    public DateTime? NextScheduledDate { get; init; }
    public int? PerformedBy { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
