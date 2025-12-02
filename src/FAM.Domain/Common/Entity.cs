namespace FAM.Domain.Common;

/// <summary>
/// Entity - child entity within an aggregate
/// Inherits all audit fields from BaseEntity
/// </summary>
public abstract class Entity : BaseEntity
{
    protected Entity() : base()
    {
    }

    protected Entity(long id) : base(id)
    {
    }
}