namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for Location
/// </summary>
public class LocationEf : EntityEf
{
    public string Name { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public CompanyEf? Company { get; set; }
}