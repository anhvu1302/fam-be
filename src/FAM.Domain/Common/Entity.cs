namespace FAM.Domain.Common;

/// <summary>
/// Base entity using long for ID
/// </summary>
public abstract class Entity : BaseEntity
{
    // Audit fields
    public new long? CreatedById { get; set; }
    public new long? UpdatedById { get; set; }
    public new long? DeletedById { get; set; }

    protected Entity() { }

    protected Entity(long id) : base(id) { }

    public override void SoftDelete(long? deletedById = null)
    {
        base.SoftDelete(deletedById);
        DeletedById = deletedById;
        UpdatedById = deletedById;
    }

    public override void Restore()
    {
        base.Restore();
        DeletedById = null;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == 0 || other.Id == 0)
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity? a, Entity? b)
    {
        return !(a == b);
    }
}
