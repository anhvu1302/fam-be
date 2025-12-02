namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// Base entity for EF Core persistence with generic TId
/// Mirrors domain BaseEntity<TId> structure
/// </summary>
public abstract class BaseEntityEf<TId> where TId : struct
{
    public TId Id { get; set; }

    // Audit fields - sync with domain BaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    protected BaseEntityEf()
    {
    }

    protected BaseEntityEf(TId id)
    {
        Id = id;
    }
}

/// <summary>
/// Base entity with long ID (most common case)
/// </summary>
public abstract class BaseEntityEf : BaseEntityEf<long>
{
    protected BaseEntityEf() : base()
    {
    }

    protected BaseEntityEf(long id) : base(id)
    {
    }
}

/// <summary>
/// Base class for junction tables (many-to-many relationships)
/// Junction tables use composite keys, not auto-increment Id
/// </summary>
public abstract class JunctionEntityEf
{
    // Only simple audit field
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    protected JunctionEntityEf()
    {
    }
}