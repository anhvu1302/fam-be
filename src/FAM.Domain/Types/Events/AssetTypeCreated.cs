using FAM.Domain.Common;

namespace FAM.Domain.Types.Events;

public sealed record AssetTypeCreated : IDomainEvent
{
    public int AssetTypeId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
