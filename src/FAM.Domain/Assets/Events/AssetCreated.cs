using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Tài sản được tạo mới
/// </summary>
public sealed record AssetCreated : IDomainEvent
{
    public required long AssetId { get; init; }
    public string AssetName { get; init; } = string.Empty;
    public string? AssetTag { get; init; }
    public int? CompanyId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
