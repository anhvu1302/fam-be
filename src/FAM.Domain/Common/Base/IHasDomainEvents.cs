namespace FAM.Domain.Common.Base;

/// <summary>
/// Interface cho entities có domain events
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}