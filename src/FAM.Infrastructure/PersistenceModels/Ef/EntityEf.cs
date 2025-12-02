namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// Entity with audit user navigation properties for EF Core
/// Audit fields (CreatedById, UpdatedById, DeletedById) inherited from BaseEntityEf
/// </summary>
public abstract class EntityEf : BaseEntityEf
{
    // Navigation properties (EF only - not in domain)
    public UserEf? CreatedBy { get; set; }
    public UserEf? UpdatedBy { get; set; }
    public UserEf? DeletedBy { get; set; }

    protected EntityEf() : base()
    {
    }

    protected EntityEf(long id) : base(id)
    {
    }
}