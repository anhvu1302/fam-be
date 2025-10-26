using FAM.Domain.Common;

namespace FAM.Domain.Geography.Events;

public sealed record CountryCreated : IDomainEvent
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
