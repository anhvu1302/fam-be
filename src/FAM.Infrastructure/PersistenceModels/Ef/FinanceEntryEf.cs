namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for FinanceEntry
/// </summary>
public class FinanceEntryEf : EntityEf
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public long UserId { get; set; }
    public UserEf? User { get; set; }
}