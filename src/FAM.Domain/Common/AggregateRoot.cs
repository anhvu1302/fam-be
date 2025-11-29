namespace FAM.Domain.Common;

/// <summary>
/// Aggregate Root - entity có thể chứa domain events (bao gồm soft delete)
/// </summary>
public abstract class AggregateRoot : Entity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(long id) : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override void SoftDelete(long? deletedBy = null)
    {
        base.SoftDelete(deletedBy);
        // Có thể raise domain event nếu cần
    }
}