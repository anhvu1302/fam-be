using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Users;

/// <summary>
/// Người dùng
/// </summary>
public class User : BaseEntity
{
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string? FullName { get; private set; }

    // Navigation properties
    public ICollection<Assets.Asset> OwnedAssets { get; set; } = new List<Assets.Asset>();
    public ICollection<Assets.Assignment> Assignments { get; set; } = new List<Assets.Assignment>();
    public ICollection<Assets.AssetEvent> AssetEvents { get; set; } = new List<Assets.AssetEvent>();
    public ICollection<Finance.FinanceEntry> FinanceEntries { get; set; } = new List<Finance.FinanceEntry>();
    public ICollection<Assets.Attachment> Attachments { get; set; } = new List<Assets.Attachment>();

    private User() { }

    public static User Create(string username, string email, string? fullName = null)
    {
        return new User
        {
            Username = Username.Create(username),
            Email = Email.Create(email),
            FullName = fullName
        };
    }
}
