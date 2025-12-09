namespace FAM.Domain.Common.Base;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract record DomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();
}