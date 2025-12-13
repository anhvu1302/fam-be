namespace FAM.Domain.Common.Base;

/// <summary>
/// Base interface cho domain events
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
