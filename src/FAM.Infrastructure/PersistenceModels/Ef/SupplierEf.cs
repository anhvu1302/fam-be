namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for Supplier
/// </summary>
public class SupplierEf : EntityEf
{
    public string Name { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public CompanyEf? Company { get; set; }
}