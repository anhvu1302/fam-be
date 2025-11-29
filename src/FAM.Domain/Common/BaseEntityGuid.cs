namespace FAM.Domain.Common;

/// <summary>
/// Base entity with GUID primary key for high-volume tables
/// </summary>
public abstract class BaseEntityGuid
{
    public Guid Id { get; protected set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    protected BaseEntityGuid()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntityGuid(Guid id)
    {
        Id = id;
    }

    public virtual void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        DeletedById = deletedById;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntityGuid other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    public static bool operator ==(BaseEntityGuid? a, BaseEntityGuid? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(BaseEntityGuid? a, BaseEntityGuid? b)
    {
        return !(a == b);
    }
}