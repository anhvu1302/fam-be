using System.ComponentModel.DataAnnotations.Schema;
using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for UserTheme
/// </summary>
[Table("user_themes")]
public class UserThemeEf : BaseEntityEf
{
    public long UserId { get; set; }

    // Theme selection
    public string Theme { get; set; } = "System"; // System, Light, Dark, Leaf, Blossom, BlueJelly

    // Primary color (hex code or preset)
    public string? PrimaryColor { get; set; }

    // Transparency level (0.0 to 1.0)
    public decimal Transparency { get; set; } = 0.5m;

    // Border radius (pixels)
    public int BorderRadius { get; set; } = 8;

    // Dark theme toggle
    public bool DarkTheme { get; set; }

    // Pin navbar
    public bool PinNavbar { get; set; }

    // Compact mode
    public bool CompactMode { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }
    public UserEf User { get; set; } = null!;
}