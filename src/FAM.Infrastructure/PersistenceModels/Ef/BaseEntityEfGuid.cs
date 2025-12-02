namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// Base entity with GUID primary key for high-volume tables
/// Inherits from generic BaseEntityEf<Guid>
/// </summary>
public abstract class BaseEntityEfGuid : BaseEntityEf<Guid>
{
    protected BaseEntityEfGuid() : base()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntityEfGuid(Guid id) : base(id)
    {
    }
}