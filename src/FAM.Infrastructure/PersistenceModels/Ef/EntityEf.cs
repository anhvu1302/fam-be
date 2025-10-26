namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// Entity with audit user navigation properties for EF Core
/// </summary>
public abstract class EntityEf : BaseEntityEf
{
    // Audit user IDs (from Domain)
    public long? CreatedById { get; set; }
    public long? UpdatedById { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties (EF only)
    public UserEf? CreatedBy { get; set; }
    public UserEf? UpdatedBy { get; set; }
    public UserEf? DeletedBy { get; set; }

    protected EntityEf() { }

    protected EntityEf(long id) : base(id) { }
}
