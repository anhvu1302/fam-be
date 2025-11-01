using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Users.Entities;

/// <summary>
/// Thiết bị đăng nhập của người dùng
/// Uses GUID as primary key for better scalability with high volume of records
/// </summary>
public class UserDevice : BaseEntityGuid
{
    public long UserId { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = string.Empty;
    public string DeviceType { get; private set; } = string.Empty; // "mobile", "desktop", "tablet", "browser"
    public string? UserAgent { get; private set; }
    public IPAddress? IpAddress { get; private set; }
    public string? Location { get; private set; } // City, Country
    public string? Browser { get; private set; }
    public string? OperatingSystem { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsTrusted { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    // Navigation property
    public User User { get; set; } = null!;

    private UserDevice() { }

    public static UserDevice Create(
        long userId,
        string deviceId,
        string deviceName,
        string deviceType,
        string? userAgent = null,
        string? ipAddress = null,
        string? location = null,
        string? browser = null,
        string? operatingSystem = null)
    {
        var device = new UserDevice
        {
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            DeviceType = deviceType,
            UserAgent = userAgent,
            Location = location,
            Browser = browser,
            OperatingSystem = operatingSystem,
            LastLoginAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            IsTrusted = false
        };

        if (!string.IsNullOrEmpty(ipAddress))
        {
            device.IpAddress = IPAddress.Create(ipAddress);
        }

        return device;
    }

    public void UpdateActivity(string? ipAddress = null, string? location = null)
    {
        LastActivityAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(ipAddress))
            IpAddress = IPAddress.Create(ipAddress);
        if (!string.IsNullOrEmpty(location))
            Location = location;
    }

    /// <summary>
    /// Record a new login event - updates LastLoginAt
    /// </summary>
    public void RecordLogin(string? ipAddress = null, string? location = null)
    {
        LastLoginAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(ipAddress))
            IpAddress = IPAddress.Create(ipAddress);
        if (!string.IsNullOrEmpty(location))
            Location = location;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiresAt)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;
    }

    /// <summary>
    /// Update refresh token - combines SetRefreshToken and RecordLogin
    /// </summary>
    public void UpdateRefreshToken(string refreshToken, DateTime expiresAt, string? ipAddress = null, string? location = null)
    {
        SetRefreshToken(refreshToken, expiresAt);
        RecordLogin(ipAddress, location);
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public void MarkAsTrusted()
    {
        IsTrusted = true;
    }

    public void RevokeTrust()
    {
        IsTrusted = false;
    }

    public void Deactivate()
    {
        IsActive = false;
        ClearRefreshToken();
    }

    public void Reactivate()
    {
        IsActive = true;
    }

    public bool IsRefreshTokenValid()
    {
        return !string.IsNullOrEmpty(RefreshToken) &&
               RefreshTokenExpiresAt.HasValue &&
               RefreshTokenExpiresAt.Value > DateTime.UtcNow;
    }
}