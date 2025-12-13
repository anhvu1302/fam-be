namespace FAM.Infrastructure.PersistenceModels.Ef.Base;

/// <summary>
/// Base entity for EF Core persistence with generic TId
/// Contains ONLY Id - mirrors domain BaseEntity<TId> structure
/// </summary>
public abstract class BaseEntityEf<TId> where TId : struct
{
    public TId Id { get; set; }

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
