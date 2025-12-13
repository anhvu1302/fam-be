using FAM.Domain.Common.Base;

namespace FAM.Domain.Locations.Events;

public sealed record LocationCreated : IDomainEvent
{
    public int LocationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public int? ParentId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
