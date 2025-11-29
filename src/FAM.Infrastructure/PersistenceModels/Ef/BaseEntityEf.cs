namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// Base entity
/// </summary>
public abstract class BaseEntityEf
{
    public long Id { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    protected BaseEntityEf()
    {
    }

    protected BaseEntityEf(long id)
    {
        Id = id;
    }
}