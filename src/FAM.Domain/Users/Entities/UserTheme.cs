using FAM.Domain.Common;

namespace FAM.Domain.Users.Entities;

/// <summary>
/// User theme preferences for UI customization
/// </summary>
public class UserTheme : BaseEntity
{
    public long UserId { get; private set; }
    
    // Theme selection
    public string Theme { get; private set; } = "System"; // System, Light, Dark, Leaf, Blossom, BlueJelly
    
    // Primary color (hex code or preset)
    public string? PrimaryColor { get; private set; }
    
    // Transparency level (0.0 to 1.0)
    public decimal Transparency { get; private set; } = 0.5m;
    
    // Border radius (pixels)
    public int BorderRadius { get; private set; } = 8;
    
    // Dark theme toggle
    public bool DarkTheme { get; private set; }
    
    // Pin navbar
    public bool PinNavbar { get; private set; }
    
    // Compact mode
    public bool CompactMode { get; private set; }

    // Navigation properties
    public User User { get; set; } = null!;

    private UserTheme()
    {
    }

    public static UserTheme CreateDefault(long userId)
    {
        return new UserTheme
        {
            UserId = userId,
            Theme = "System",
            PrimaryColor = "#2563EB", // Blue
            Transparency = 0.5m,
            BorderRadius = 8,
            DarkTheme = false,
            PinNavbar = false,
            CompactMode = false
        };
    }

    public static UserTheme Create(
        long userId,
        string theme,
        string? primaryColor = null,
        decimal transparency = 0.5m,
        int borderRadius = 8,
        bool darkTheme = false,
        bool pinNavbar = false,
        bool compactMode = false)
    {
        return new UserTheme
        {
            UserId = userId,
            Theme = theme,
            PrimaryColor = primaryColor,
            Transparency = transparency,
            BorderRadius = borderRadius,
            DarkTheme = darkTheme,
            PinNavbar = pinNavbar,
            CompactMode = compactMode
        };
    }

    public void UpdateTheme(
        string theme,
        string? primaryColor,
        decimal transparency,
        int borderRadius,
        bool darkTheme,
        bool pinNavbar,
        bool compactMode)
    {
        Theme = theme;
        PrimaryColor = primaryColor;
        Transparency = transparency;
        BorderRadius = borderRadius;
        DarkTheme = darkTheme;
        PinNavbar = pinNavbar;
        CompactMode = compactMode;
    }
}
