using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for UserDevice
/// </summary>
[Table("user_devices")]
public class UserDeviceEf : BaseEntityEf
{
    public long UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTrusted { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation properties for EF relationships
    public UserEf User { get; set; } = null!;
}