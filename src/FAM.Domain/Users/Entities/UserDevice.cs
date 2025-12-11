using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Users.Entities;

/// <summary>
/// Thiết bị đăng nhập của người dùng
/// Uses GUID as primary key for better scalability with high volume of records
/// Uses basic audit trail
/// </summary>
public class UserDevice : BaseEntityGuid, IHasCreationTime, IHasCreator, IHasModificationTime, IHasDeletionTime
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

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
    public string? ActiveAccessTokenJti { get; private set; }

    // Navigation property
    public User User { get; set; } = null!;

    private UserDevice()
    {
    }

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

        if (!string.IsNullOrEmpty(ipAddress)) device.IpAddress = IPAddress.Create(ipAddress);

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
    public void UpdateRefreshToken(string refreshToken, DateTime expiresAt, string? ipAddress = null,
        string? location = null)
    {
        SetRefreshToken(refreshToken, expiresAt);
        RecordLogin(ipAddress, location);
    }

    /// <summary>
    /// Set the JTI of the currently active access token
    /// This allows immediate revocation when session is deleted
    /// </summary>
    public void SetActiveAccessTokenJti(string jti)
    {
        ActiveAccessTokenJti = jti;
    }
    
    /// <summary>
    /// Update both refresh token and active access token JTI
    /// Used during token refresh to track the new access token
    /// </summary>
    public void UpdateTokens(string refreshToken, DateTime refreshTokenExpiresAt, string accessTokenJti, 
        string? ipAddress = null, string? location = null)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        ActiveAccessTokenJti = accessTokenJti;
        RecordLogin(ipAddress, location);
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
        ActiveAccessTokenJti = null;
    }

    public void MarkAsTrusted()
    {
        IsTrusted = true;
    }

    public void RevokeTrust()
    {
        IsTrusted = false;
    }

    /// <summary>
    /// Check if device has been trusted for at least the specified number of days
    /// This is a security measure to prevent newly compromised devices from deleting other sessions
    /// </summary>
    /// <param name="minimumDays">Minimum number of days device must be trusted (default 3)</param>
    /// <returns>True if device is trusted and has been for the minimum period</returns>
    public bool IsTrustedForDuration(int minimumDays = 3)
    {
        if (!IsTrusted) return false;

        // Device must have been created at least minimumDays ago
        var trustDuration = DateTime.UtcNow - CreatedAt;
        return trustDuration.TotalDays >= minimumDays;
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

    public virtual void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}