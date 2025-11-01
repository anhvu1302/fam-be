namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// Base entity with GUID primary key for high-volume tables
/// </summary>
public abstract class BaseEntityEfGuid
{
    public Guid Id { get; protected set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    protected BaseEntityEfGuid()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntityEfGuid(Guid id)
    {
        Id = id;
    }
}
