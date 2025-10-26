namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for Asset
/// </summary>
public class AssetEf : EntityEf
{
    public string Name { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public CompanyEf? Company { get; set; }
    public long? UserEfId { get; set; }
    public UserEf? UserEf { get; set; }
}