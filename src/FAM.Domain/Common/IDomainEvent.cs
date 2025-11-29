namespace FAM.Domain.Common;

/// <summary>
/// Base interface cho domain events
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}