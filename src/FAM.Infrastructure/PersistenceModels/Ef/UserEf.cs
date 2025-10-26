using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for User
/// Separate from domain entity to avoid persistence concerns leaking into domain
/// </summary>
[Table("users")]
public class UserEf : BaseEntityEf
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }

    // Navigation properties for EF relationships
    public ICollection<AssetEf> OwnedAssets { get; set; } = new List<AssetEf>();
    public ICollection<FinanceEntryEf> FinanceEntries { get; set; } = new List<FinanceEntryEf>();
}